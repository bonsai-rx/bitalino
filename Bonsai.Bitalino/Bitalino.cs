using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Bitalino
{
    public class Bitalino : Source<BitalinoDataFrame>
    {
        static readonly int[] EmptyChannels = new int[0];

        public Bitalino()
        {
            BatteryThreshold = 10;
            BufferLength = 100;
            SamplingRate = 1000;
        }

        [TypeConverter(typeof(AddressConverter))]
        public string Address { get; set; }

        public int BatteryThreshold { get; set; }

        public int SamplingRate { get; set; }

        public int BufferLength { get; set; }

        [TypeConverter(typeof(UnidimensionalArrayConverter))]
        [Description("The channels to include in the output buffer. Reordering and duplicating channels is allowed.")]
        public int[] Channels { get; set; }

        public bool Simulated { get; set; }

        public override IObservable<BitalinoDataFrame> Generate()
        {
            return Observable.Create<BitalinoDataFrame>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        using (var bitalino = new global::Bitalino(Address))
                        {
                            bitalino.battery(BatteryThreshold);
                            bitalino.start(SamplingRate, Channels ?? EmptyChannels, Simulated);

                            var frames = new global::Bitalino.Frame[BufferLength];
                            for (int i = 0; i < frames.Length; i++)
                            {
                                frames[i] = new global::Bitalino.Frame();
                            }

                            var ledState = new bool[4];
                            ledState[2] = true;
                            bitalino.trigger(ledState);

                            do
                            {
                                bitalino.read(frames);

                                var output = new BitalinoDataFrame(frames);
                                observer.OnNext(output);
                            }
                            while (!cancellationToken.IsCancellationRequested);

                            ledState[2] = false;
                            bitalino.trigger(ledState);
                            bitalino.stop();
                        }
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }

        class AddressConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var devices = global::Bitalino.find();
                return new StandardValuesCollection(Array.ConvertAll(devices, device => device.macAddr));
            }
        }
    }
}
