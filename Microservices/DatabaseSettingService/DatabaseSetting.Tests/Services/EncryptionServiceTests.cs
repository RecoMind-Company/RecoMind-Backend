using DatabaseSetting.Core.Services;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using Moq;

namespace DatabaseSetting.Tests.Services
{
    public class EncryptionServiceTests
    {
        private const string ValidKey = "12345678901234567890123456789012"; // 32 bytes
        private const string ValidIV = "1234567890123456";                 // 16 bytes

        private static EncryptionService BuildSut(string key = ValidKey, string iv = ValidIV)
        {
            var config = new Mock<IConfiguration>();
            config.Setup(c => c["Encryption:Key"]).Returns(key);
            config.Setup(c => c["Encryption:IV"]).Returns(iv);
            return new EncryptionService(config.Object);
        }

        // Encrypt
        #region Encrypt Tests
        [Fact]
        public void Encrypt_ShouldReturnValidBase64_WhenInputIsValid()
        {
            var sut = BuildSut();
            var result = sut.Encrypt("MySecretPassword123");

            result.Should().NotBeNullOrWhiteSpace();
            result.Should().NotBe("MySecretPassword123");
            Action act = () => Convert.FromBase64String(result);
            act.Should().NotThrow("output must be valid Base64");
        }

        [Fact]
        public void Encrypt_ShouldProduceDifferentCipherText_ForDifferentInputs()
        {
            var sut = BuildSut();

            sut.Encrypt("Password1").Should().NotBe(sut.Encrypt("Password2"));
        }

        [Fact]
        public void Encrypt_ShouldProduceSameCipherText_ForSameInputWithFixedIV()
        {
            var sut = BuildSut();

            sut.Encrypt("hello").Should().Be(sut.Encrypt("hello"));
        }

        [Fact]
        public void Encrypt_ShouldHandleEmptyString_WithoutThrowing()
        {
            var sut = BuildSut();

            Action act = () => sut.Encrypt(string.Empty);
            act.Should().NotThrow();
        }
        #endregion

        // Decrypt
        #region Decrypt Tests
        [Fact]
        public void Decrypt_ShouldReturnOriginalPlainText_AfterEncrypt()
        {
            var sut = BuildSut();
            var cipherText = sut.Encrypt("MySecretPassword123");

            sut.Decrypt(cipherText).Should().Be("MySecretPassword123");
        }

        [Theory]
        [InlineData("short")]
        [InlineData("a longer password with spaces and $ymbols!")]
        [InlineData("1234567890")]
        public void Decrypt_ShouldRoundTrip_ForVariousInputs(string plainText)
        {
            var sut = BuildSut();

            sut.Decrypt(sut.Encrypt(plainText)).Should().Be(plainText);
        }

        [Fact]
        public void Decrypt_ShouldThrow_WhenCipherTextIsNotValidBase64()
        {
            var sut = BuildSut();

            Action act = () => sut.Decrypt("this is not base64!!!");
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Decrypt_ShouldThrow_WhenCipherTextWasEncryptedWithDifferentKey()
        {
            var sut1 = BuildSut(key: "12345678901234567890123456789012");
            var sut2 = BuildSut(key: "99999999999999999999999999999999");
            var cipherText = sut1.Encrypt("secret");

            Action act = () => sut2.Decrypt(cipherText);
            act.Should().Throw<Exception>();
        }
        #endregion
    }
}