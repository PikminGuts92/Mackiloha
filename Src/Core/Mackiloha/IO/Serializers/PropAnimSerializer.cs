using Mackiloha.Song;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.IO.Serializers
{
    public class PropAnimSerializer : AbstractSerializer
    {
        public PropAnimSerializer(MiloSerializer miloSerializer) : base(miloSerializer) { }

        public override void ReadFromStream(AwesomeReader ar, ISerializable data)
        {
            var propAnim = data as PropAnim;
            int version = ReadMagic(ar, data);

            var meta = ReadMeta(ar);
            propAnim.AnimName = meta.ScriptName;

            ar.BaseStream.Position += 4; // Skip 4 constant
            propAnim.TotalTime = ar.ReadSingle();
            ar.BaseStream.Position += 4; // Skip 0/1 number

            if (version >= 12)
                ar.BaseStream.Position += 1;

            var eventGroupCount = ar.ReadInt32();
            propAnim.DirectorGroups
                .AddRange(
                    RepeatFor(eventGroupCount, () => ReadGroupEvent(ar)));
        }

        protected virtual DirectedEventGroup ReadGroupEvent(AwesomeReader ar)
        {
            var eventGroup = new DirectedEventGroup();

            var num1 = ar.ReadInt32();
            var num2 = ar.ReadInt32();

            if (num1 != num2)
                throw new NotSupportedException($"Expected \"{num1}\" to equal \"{num2}\" for start of directed event group");

            if (!Enum.IsDefined(typeof(DirectedEventType), num1))
                throw new NotSupportedException($"Value of \'{num1}\' is not supported for directed event!");

            eventGroup.EventType = (DirectedEventType)num1;
            eventGroup.DirectorName = ar.ReadString();
            ar.BaseStream.Position += 11; // Skip constants

            eventGroup.PropName = ar.ReadString();
            ar.BaseStream.Position += 4; // Always 0?
            eventGroup.PropName2 = ar.ReadString(); // Usually empty

            ar.BaseStream.Position += 4; // Unknown enum... hopefully it's not important

            var count = ar.ReadInt32();
            eventGroup.Events = new List<IDirectedEvent>();

            switch (eventGroup.EventType)
            {
                case DirectedEventType.Float:
                    eventGroup.Events.AddRange(RepeatFor<IDirectedEvent>(count,
                        () => new DirectedEventFloat()
                        {
                            Value = ar.ReadSingle(),
                            Position = ar.ReadSingle()
                        }));
                    break;
                case DirectedEventType.TextFloat:
                    eventGroup.Events.AddRange(RepeatFor<IDirectedEvent>(count,
                        () => new DirectedEventTextFloat()
                        {
                            Value = ar.ReadSingle(),
                            Text = ar.ReadString(),
                            Position = ar.ReadSingle()
                        }));
                    break;
                case DirectedEventType.Boolean:
                    eventGroup.Events.AddRange(RepeatFor<IDirectedEvent>(count,
                        () => new DirectedEventBoolean()
                        {
                            Enabled = ar.ReadBoolean(),
                            Position = ar.ReadSingle()
                        }));
                    break;
                case DirectedEventType.Vector4:
                    eventGroup.Events.AddRange(RepeatFor<IDirectedEvent>(count,
                        () => new DirectedEventVector4()
                        {
                            Value = new Vector4()
                            {
                                X = ar.ReadSingle(),
                                Y = ar.ReadSingle(),
                                Z = ar.ReadSingle(),
                                W = ar.ReadSingle()
                            },
                            Position = ar.ReadSingle()
                        }));
                    break;
                case DirectedEventType.Vector3:
                    eventGroup.Events.AddRange(RepeatFor<IDirectedEvent>(count,
                        () => new DirectedEventVector3()
                        {
                            Value = new Vector3()
                            {
                                X = ar.ReadSingle(),
                                Y = ar.ReadSingle(),
                                Z = ar.ReadSingle()
                            },
                            Position = ar.ReadSingle()
                        }));
                    break;
                case DirectedEventType.Text:
                    eventGroup.Events.AddRange(RepeatFor<IDirectedEvent>(count,
                        () => new DirectedEventText()
                        {
                            Text = ar.ReadString(),
                            Position = ar.ReadSingle()
                        }));
                    break;
            }

            return eventGroup;
        }

        public override void WriteToStream(AwesomeWriter aw, ISerializable data)
        {
            throw new NotImplementedException();
        }

        public override bool IsOfType(ISerializable data) => data is PropAnim;

        public override int Magic()
        {
            switch (MiloSerializer.Info.Version)
            {
                case 25:
                    // TBRB
                    // TODO: Refector to use optional different magic
                    return 11;
                default:
                    return -1;
            }
        }

        internal override int[] ValidMagics()
        {
            switch (MiloSerializer.Info.Version)
            {
                case 25:
                    // TBRB / GDRB
                    return new[] { 11, 12 };
                default:
                    return Array.Empty<int>();
            }
        }
    }
}
