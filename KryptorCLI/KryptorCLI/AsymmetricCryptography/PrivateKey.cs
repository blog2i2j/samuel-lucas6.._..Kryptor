﻿using System;
using Sodium;
using System.Security.Cryptography;

/*
    Kryptor: Free and open source file encryption.
    Copyright(C) 2020-2021 Samuel Lucas

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program. If not, see https://www.gnu.org/licenses/.
*/

namespace KryptorCLI
{
    public static class PrivateKey
    {
        public static byte[] Encrypt(byte[] passwordBytes, byte[] privateKey)
        {
            byte[] memorySize = FileHeaders.GetMemorySize();
            byte[] iterations = FileHeaders.GetIterations();
            byte[] argon2Parameters = Utilities.ConcatArrays(memorySize, iterations);
            byte[] salt = Generate.RandomSalt();
            byte[] key = Argon2.DeriveKey(passwordBytes, salt);
            Utilities.ZeroArray(passwordBytes);
            byte[] nonce = Generate.RandomNonce();
            byte[] keyCommitmentBlock = ChunkHandling.GetKeyCommitmentBlock();
            privateKey = Utilities.ConcatArrays(keyCommitmentBlock, privateKey);
            byte[] encryptedPrivateKey = SecretAeadXChaCha20Poly1305.Encrypt(privateKey, nonce, key, argon2Parameters);
            Utilities.ZeroArray(privateKey);
            Utilities.ZeroArray(key);
            return Utilities.ConcatArrays(argon2Parameters, salt, nonce, encryptedPrivateKey);
        }

        public static byte[] Decrypt(byte[] privateKey)
        {
            try
            {
                char[] password = PasswordPrompt.EnterYourPassword();
                byte[] passwordBytes = Password.Hash(password);
                return Decrypt(passwordBytes, privateKey);
            }
            catch (CryptographicException)
            {
                DisplayMessage.Error("Incorrect password or the private key has been tampered with.");
                return null;
            }
        }

        private static byte[] Decrypt(byte[] passwordBytes, byte[] privateKey)
        {
            byte[] argon2Parameters = GetArgon2Parameters(privateKey);
            byte[] salt = GetSalt(privateKey);
            byte[] nonce = GetNonce(privateKey);
            byte[] encryptedPrivateKey = GetEncryptedPrivateKey(privateKey);
            byte[] key = Argon2.DeriveKey(passwordBytes, salt);
            Utilities.ZeroArray(passwordBytes);
            byte[] decryptedPrivateKey = SecretAeadXChaCha20Poly1305.Decrypt(encryptedPrivateKey, nonce, key, argon2Parameters);
            Utilities.ZeroArray(key);
            ChunkHandling.ValidateKeyCommitmentBlock(decryptedPrivateKey);
            return ChunkHandling.RemoveKeyCommitmentBlock(decryptedPrivateKey);
        }

        private static byte[] GetArgon2Parameters(byte[] privateKey)
        {
            byte[] argon2Parameters = new byte[Constants.BitConverterLength * 2];
            Array.Copy(privateKey, argon2Parameters, argon2Parameters.Length);
            return argon2Parameters;
        }

        private static byte[] GetSalt(byte[] privateKey)
        {
            byte[] salt = new byte[Constants.SaltLength];
            int sourceIndex = Constants.BitConverterLength * 2;
            Array.Copy(privateKey, sourceIndex, salt, destinationIndex: 0, salt.Length);
            return salt;
        }

        private static byte[] GetNonce(byte[] privateKey)
        {
            byte[] nonce = new byte[Constants.XChaChaNonceLength];
            int sourceIndex = (Constants.BitConverterLength * 2) + Constants.SaltLength;
            Array.Copy(privateKey, sourceIndex, nonce, destinationIndex: 0, nonce.Length);
            return nonce;
        }

        private static byte[] GetEncryptedPrivateKey(byte[] privateKey)
        {
            byte[] encryptedKey = new byte[Constants.EncryptedPrivateKeyLength];
            int sourceIndex = (Constants.BitConverterLength * 2) + Constants.SaltLength + Constants.XChaChaNonceLength;
            Array.Copy(privateKey, sourceIndex, encryptedKey, destinationIndex: 0, encryptedKey.Length);
            return encryptedKey;
        }
    }
}
