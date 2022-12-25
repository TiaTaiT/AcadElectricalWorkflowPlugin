using System.Collections.Generic;

namespace LinkCommands.Models
{
    public static class LinkStruct
    {
        public readonly struct Left
        {
            public const string SourceBlockName = "ha2s1_inline";
            public const string DestinationBlockName = "ha2d1_inline";
            public const string AttributeMultiwire = "X1TERM02";
            public const string AttributeWire = "X4TERM01";
        }

        public readonly struct Down
        {
            public const string SourceBlockName = "ha2s2_inline";
            public const string DestinationBlockName = "ha2d2_inline";
            public const string AttributeMultiwire = "X2TERM02";
            public const string AttributeWire = "X8TERM01";
        }

        public readonly struct Right
        {
            public const string SourceBlockName = "ha2s3_inline";
            public const string DestinationBlockName = "ha2d3_inline";
            public const string AttributeMultiwire = "X4TERM02";
            public const string AttributeWire = "X1TERM01";
        }

        public readonly struct Up
        {
            public const string SourceBlockName = "ha2s4_inline";
            public const string DestinationBlockName = "ha2d4_inline";
            public const string AttributeMultiwire = "X8TERM02";
            public const string AttributeWire = "X2TERM01";
        }

        public readonly struct Circle
        {
            public const string AttributeWire = "X0TERM";
        }

        public static IEnumerable<string> GetAllLinkNames()
        {
            yield return Left.SourceBlockName;
            yield return Down.SourceBlockName;
            yield return Right.SourceBlockName;
            yield return Up.SourceBlockName;
            yield return Left.DestinationBlockName;
            yield return Down.DestinationBlockName;
            yield return Right.DestinationBlockName;
            yield return Up.DestinationBlockName;
        }

        public static IEnumerable<string> GetPossibleMultiwireAttachedNames()
        {
            yield return Left.AttributeMultiwire;
            yield return Down.AttributeMultiwire;
            yield return Right.AttributeMultiwire;
            yield return Up.AttributeMultiwire;
        }

        public static IEnumerable<string> GetPossibleWireAttachedNames()
        {
            yield return Left.AttributeWire;
            yield return Down.AttributeWire;
            yield return Right.AttributeWire;
            yield return Up.AttributeWire;
        }
    }
}
