using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IMessageEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string encryptedText);
    }
}
