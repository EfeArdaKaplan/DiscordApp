using Business.Abstract;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Business.Services
{
    // mesaj içeriği AES-256-GCM ile şifrelicezz
    // gcm hem şifreleme hemde yolda değiştirildimi kontrolü yapar, tek algoritmada
    public class MessageEncryptionService : IMessageEncryptionService
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;

        private readonly byte[] _key;

        public MessageEncryptionService(IConfiguration config)
        {
            var keyBase64 = config["Encryption:Key"]
                ?? throw new InvalidOperationException("Encryption:Key appsetings.json içinde tanımlı değil.");

            _key = Convert.FromBase64String(keyBase64);
            if (_key.Length != 32)
                throw new InvalidOperationException("Encryption:Key tam olarak 32 byte (256 bit) olmalı");
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            var nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            var cipherBytes = new byte[plainBytes.Length];
            var tag = new byte[TagSize];

            using var aesGcm = new AesGcm(_key, TagSize);
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

            var result = new byte[NonceSize + TagSize + cipherBytes.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
            Buffer.BlockCopy(cipherBytes, 0, result, NonceSize + TagSize, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText)) return encryptedText;

            byte[] fullBytes;
            try
            {
                fullBytes = Convert.FromBase64String(encryptedText);
            } catch (FormatException)
            {
                return encryptedText;
            }

            if (fullBytes.Length < NonceSize + TagSize) return encryptedText;

            var nonce = fullBytes[..NonceSize];
            var tag = fullBytes[NonceSize..(NonceSize + TagSize)];
            var cipherBytes = fullBytes[(NonceSize + TagSize)..];

            var plainBytes = new byte[cipherBytes.Length];

            try
            {
                using var aesGscm = new AesGcm(_key, TagSize);
                aesGscm.Decrypt(nonce, cipherBytes, tag, plainBytes);
            } catch(CryptographicException)
            {
                return "Bu mesajın bütünlüğü doğrulanamadı.";
            }

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
