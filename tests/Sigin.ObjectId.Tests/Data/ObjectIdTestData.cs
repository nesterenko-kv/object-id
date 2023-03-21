using System.Diagnostics.CodeAnalysis;
using Sigin.ObjectId.Tests.Data.Models;

namespace Sigin.ObjectId.Tests.Data;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public static class ObjectIdTestData
{
    public static object[] CorrectTimestamps { get; } =
    {
        new object[] {0xffffffff, new DateTimeOffset(year: 2106, month: 2, day: 7, hour: 6, minute: 28, second: 15, TimeSpan.Zero)},
        new object[] {0x80000000, new DateTimeOffset(year: 2038, month: 1, day: 19, hour: 3, minute: 14, second: 08, TimeSpan.Zero)},
        new object[] {0x7FFFFFFF, new DateTimeOffset(year: 2038, month: 1, day: 19, hour: 3, minute: 14, second: 07, TimeSpan.Zero)},
        new object[] {0x00000000, new DateTimeOffset(year: 1970, month: 1, day: 1, hour: 0, minute: 0, second: 0, TimeSpan.Zero)}
    };

    public static object[] CorrectObjectIdBytesArrays { get; } =
    {
        new object[] {new byte[] {10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120}},
        new object[] {new byte[] {255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0}},
        new object[] {new byte[] {0, 0, 0, 255, 255, 255, 0, 0, 0, 0, 0, 0}},
        new object[] {new byte[] {0, 0, 0, 0, 0, 0, 255, 255, 255, 0, 0, 0}},
        new object[] {new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255}},
        new object[] {new byte[] {100, 5, 230, 40, 64, 55, 207, 161, 212, 8, 86, 102}},
        new object[] {new byte[] {100, 5, 236, 57, 24, 100, 30, 239, 85, 163, 87, 170}}
    };

    public static object[] CorrectObjectIdAndComponents { get; } =
    {
        new object[] {0x507F191E, 0x810C19729D, 0xE860EA, "507f191e810c19729de860ea"},
        new object[] {0x507F1F77, 0xBCF86CD799, 0x439011, "507f1f77bcf86cd799439011"},

        new object[] {0xFFFFFFFF, 0xFFFFFFFFFF, 0xFFFFFF, "ffffffffffffffffffffffff"},
        new object[] {0, 0, 0, "000000000000000000000000"},

        new object[] {0xFFFFFFFF, 0, 0, "ffffffff0000000000000000"},
        new object[] {1, 0, 0, "000000010000000000000000"},

        new object[] {0, 0xFFFFFFFFFF, 0, "00000000ffffffffff000000"},
        new object[] {0, 1, 0, "000000000000000001000000"},

        new object[] {0, 0, 0xFFFFFF, "000000000000000000ffffff"},
        new object[] {0, 0, 1, "000000000000000000000001"}
    };

    public static object[] CorrectCompareToArraysAndResult { get; } =
    {
        new object[]
        {
            new byte[] {42, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new byte[] {13, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            1
        },
        new object[]
        {
            new byte[] {13, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new byte[] {42, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            -1
        },
        new object[]
        {
            new byte[] {17, 42, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new byte[] {17, 13, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            1
        },
        new object[]
        {
            new byte[] {17, 13, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new byte[] {17, 42, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            -1
        },
        new object[]
        {
            new byte[] {29, 17, 42, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new byte[] {29, 17, 13, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            1
        },
        new object[]
        {
            new byte[] {29, 17, 13, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new byte[] {29, 17, 42, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            -1
        },
        new object[]
        {
            new byte[] {173, 29, 17, 42, 0, 0, 0, 0, 0, 0, 0, 0},
            new byte[] {173, 29, 17, 13, 0, 0, 0, 0, 0, 0, 0, 0},
            1
        },
        new object[]
        {
            new byte[] {173, 29, 17, 13, 0, 0, 0, 0, 0, 0, 0, 0},
            new byte[] {173, 29, 17, 42, 0, 0, 0, 0, 0, 0, 0, 0},
            -1
        },
        new object[]
        {
            new byte[] {234, 173, 29, 17, 42, 0, 0, 0, 0, 0, 0, 0},
            new byte[] {234, 173, 29, 17, 13, 0, 0, 0, 0, 0, 0, 0},
            1
        },
        new object[]
        {
            new byte[] {234, 173, 29, 17, 13, 0, 0, 0, 0, 0, 0, 0},
            new byte[] {234, 173, 29, 17, 42, 0, 0, 0, 0, 0, 0, 0},
            -1
        },
        new object[]
        {
            new byte[] {97, 234, 173, 29, 17, 42, 0, 0, 0, 0, 0, 0},
            new byte[] {97, 234, 173, 29, 17, 13, 0, 0, 0, 0, 0, 0},
            1
        },
        new object[]
        {
            new byte[] {97, 234, 173, 29, 17, 13, 0, 0, 0, 0, 0, 0},
            new byte[] {97, 234, 173, 29, 17, 42, 0, 0, 0, 0, 0, 0},
            -1
        },
        new object[]
        {
            new byte[] {23, 97, 234, 173, 29, 17, 42, 0, 0, 0, 0, 0},
            new byte[] {23, 97, 234, 173, 29, 17, 13, 0, 0, 0, 0, 0},
            1
        },
        new object[]
        {
            new byte[] {23, 97, 234, 173, 29, 17, 13, 0, 0, 0, 0, 0},
            new byte[] {23, 97, 234, 173, 29, 17, 42, 0, 0, 0, 0, 0},
            -1
        },
        new object[]
        {
            new byte[] {81, 23, 97, 234, 173, 29, 17, 42, 0, 0, 0, 0},
            new byte[] {81, 23, 97, 234, 173, 29, 17, 13, 0, 0, 0, 0},
            1
        },
        new object[]
        {
            new byte[] {81, 23, 97, 234, 173, 29, 17, 13, 0, 0, 0, 0},
            new byte[] {81, 23, 97, 234, 173, 29, 17, 42, 0, 0, 0, 0},
            -1
        },
        new object[]
        {
            new byte[] {125, 81, 23, 97, 234, 173, 29, 17, 42, 0, 0, 0},
            new byte[] {125, 81, 23, 97, 234, 173, 29, 17, 13, 0, 0, 0},
            1
        },
        new object[]
        {
            new byte[] {125, 81, 23, 97, 234, 173, 29, 17, 13, 0, 0, 0},
            new byte[] {125, 81, 23, 97, 234, 173, 29, 17, 42, 0, 0, 0},
            -1
        },
        new object[]
        {
            new byte[] {69, 125, 81, 23, 97, 234, 173, 29, 17, 42, 0, 0},
            new byte[] {69, 125, 81, 23, 97, 234, 173, 29, 17, 13, 0, 0},
            1
        },
        new object[]
        {
            new byte[] {69, 125, 81, 23, 97, 234, 173, 29, 17, 13, 0, 0},
            new byte[] {69, 125, 81, 23, 97, 234, 173, 29, 17, 42, 0, 0},
            -1
        },
        new object[]
        {
            new byte[] {117, 69, 125, 81, 23, 97, 234, 173, 29, 17, 42, 0},
            new byte[] {117, 69, 125, 81, 23, 97, 234, 173, 29, 17, 13, 0},
            1
        },
        new object[]
        {
            new byte[] {117, 69, 125, 81, 23, 97, 234, 173, 29, 17, 13, 0},
            new byte[] {117, 69, 125, 81, 23, 97, 234, 173, 29, 17, 42, 0},
            -1
        },
        new object[]
        {
            new byte[] {77, 117, 69, 125, 81, 23, 97, 234, 173, 29, 17, 42},
            new byte[] {77, 117, 69, 125, 81, 23, 97, 234, 173, 29, 17, 13},
            1
        },
        new object[]
        {
            new byte[] {77, 117, 69, 125, 81, 23, 97, 234, 173, 29, 17, 13},
            new byte[] {77, 117, 69, 125, 81, 23, 97, 234, 173, 29, 17, 42},
            -1
        },
        new object[]
        {
            new byte[] {1, 72, 99, 252, 201, 77, 117, 69, 125, 81, 23, 97},
            new byte[] {1, 72, 99, 252, 201, 77, 117, 69, 125, 81, 23, 97},
            0
        }
    };

    // ---------
    // --- N ---
    // ---------
    public static ObjectIdStringWithBytes[] CorrectNStrings { get; } = ObjectIdTestsUtils.GenerateNStrings();

    public static string[] LargeNStrings { get; } = CorrectNStrings
        .Select(x => x.String + "f")
        .ToArray();

    public static string[] SmallNStrings { get; } = CorrectNStrings
        .Select(x => x.String[(x.String.Length / 2)..])
        .ToArray();

    public static string[] BrokenNStrings { get; } = ObjectIdTestsUtils.GenerateBrokenNStringsArray();

    public static TestCaseData[] CorrectEqualsToBytesAndResult()
    {
        return new[]
        {
            new TestCaseData(
                new byte[] {42, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] {13, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0},
                arg3: false
            ),
            new TestCaseData(
                new byte[] {42, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new byte[] {42, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                arg3: true
            ),
            new TestCaseData(
                new byte[] {13, 1, 2, 3, 0, 0, 42, 0, 0, 0, 0, 0},
                new byte[] {13, 1, 2, 3, 0, 0, 37, 0, 0, 0, 0, 0},
                arg3: false
            ),
            new TestCaseData(
                new byte[] {13, 1, 3, 2, 0, 0, 42, 0, 0, 0, 0, 0},
                new byte[] {13, 1, 3, 2, 0, 0, 42, 0, 0, 0, 0, 0},
                arg3: true
            ),
            new TestCaseData(
                new byte[] {13, 3, 2, 1, 0, 0, 42, 0, 0, 0, 0, 255},
                new byte[] {13, 3, 2, 1, 0, 0, 42, 0, 0, 0, 0, 255},
                arg3: true
            ),
            new TestCaseData(
                new byte[] {13, 2, 3, 1, 0, 0, 42, 0, 0, 0, 251, 1},
                new byte[] {13, 2, 3, 1, 0, 0, 42, 0, 0, 0, 251, 2},
                arg3: false
            )
        };
    }

    public static object[] LeftLessThanRight()
    {
        var src = CorrectCompareToArraysAndResult;
        var results = new List<object>();
        foreach (var arg in src)
        {
            var args = (object[]) arg;
            var left = (byte[]) args[0];
            var right = (byte[]) args[1];
            var flag = (int) args[2];
            if (flag == -1)
            {
                var outputArgs = new object[]
                {
                    new ObjectId(left),
                    new ObjectId(right)
                };
                results.Add(outputArgs);
            }
        }

        return results.ToArray();
    }

    public static object[] RightLessThanLeft()
    {
        var src = CorrectCompareToArraysAndResult;
        var results = new List<object>();
        foreach (var arg in src)
        {
            var args = (object[]) arg;
            var left = (byte[]) args[0];
            var right = (byte[]) args[1];
            var flag = (int) args[2];
            if (flag == 1)
            {
                var outputArgs = new object[]
                {
                    new ObjectId(left),
                    new ObjectId(right)
                };
                results.Add(outputArgs);
            }
        }

        return results.ToArray();
    }

    public static class Formats
    {
        public static string[] All { get; } =
        {
            "N",
            "n"
        };

        public static string[] N { get; } =
        {
            "N",
            "n"
        };

        [SuppressMessage("ReSharper", "EmptyArrayInitialization")]
        public static string[] AllExceptN { get; } = Array.Empty<string>();
    }
}
