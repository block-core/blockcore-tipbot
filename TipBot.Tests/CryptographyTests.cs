using TipBot.Helpers;
using Xunit;

namespace TipBot.Tests
{
    public class CryptographyTests
    {
        [Fact]
        public void HashCalculatedCorrectly()
        {
            string hash = Cryptography.Hash("test");

            Assert.Equal("9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08", hash);
        }

        [Fact]
        public void IsHashOfDataCorrect()
        {
            Assert.True(Cryptography.IsHashOfData("test", "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08"));
            Assert.True(Cryptography.IsHashOfData("test2", "60303AE22B998861BCE3B28F33EEC1BE758A213C86C93C076DBE9F558C11C752"));
            Assert.False(Cryptography.IsHashOfData("test3", "1231A03AF4F77D870FC21E05E7E80678095C92D808CFB3B5C279EE04C74ACA13"));
        }
    }
}
