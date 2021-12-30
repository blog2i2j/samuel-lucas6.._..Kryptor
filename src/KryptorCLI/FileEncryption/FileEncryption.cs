﻿/*
    Kryptor: A simple, modern, and secure encryption tool.
    Copyright (C) 2020-2022 Samuel Lucas

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

using System;
using System.Security.Cryptography;
using Sodium;

namespace KryptorCLI;

public static class FileEncryption
{
    public static void EncryptEachFileWithPassword(string[] filePaths, byte[] passwordBytes)
    {
        if (filePaths == null || passwordBytes == null) { return; }
        foreach (string inputFilePath in filePaths)
        {
            UsingPassword(inputFilePath, passwordBytes);
        }
        CryptographicOperations.ZeroMemory(passwordBytes);
        DisplayMessage.SuccessfullyEncrypted();
    }

    private static void UsingPassword(string inputFilePath, byte[] passwordBytes)
    {
        try
        {
            if (FileHandling.IsDirectory(inputFilePath))
            {
                DirectoryEncryption.UsingPassword(inputFilePath, passwordBytes);
                return;
            }
            byte[] salt = SodiumCore.GetRandomBytes(Constants.SaltLength);
            byte[] keyEncryptionKey = KeyDerivation.Argon2id(passwordBytes, salt);
            // Fill unused header with random public key
            using var ephemeralKeyPair = PublicKeyBox.GenerateKeyPair();
            EncryptInputFile(inputFilePath, ephemeralKeyPair.PublicKey, salt, keyEncryptionKey);
        }
        catch (Exception ex) when (ExceptionFilters.Cryptography(ex))
        {
            DisplayMessage.FilePathException(inputFilePath, ex.GetType().Name, ErrorMessages.UnableToEncryptFile);
        }
    }

    private static void EncryptInputFile(string inputFilePath, byte[] ephemeralPublicKey, byte[] salt, byte[] keyEncryptionKey)
    {
        string outputFilePath = FileHandling.GetEncryptedOutputFilePath(inputFilePath);
        DisplayMessage.EncryptingFile(inputFilePath, outputFilePath);
        EncryptFile.Encrypt(inputFilePath, outputFilePath, ephemeralPublicKey, salt, keyEncryptionKey);
        CryptographicOperations.ZeroMemory(keyEncryptionKey);
    }

    public static void EncryptEachFileWithPublicKey(string[] filePaths, byte[] senderPrivateKey, byte[] recipientPublicKey)
    {
        if (filePaths == null || senderPrivateKey == null || recipientPublicKey == null) { return; }
        senderPrivateKey = PrivateKey.Decrypt(senderPrivateKey);
        if (senderPrivateKey == null) { return; }
        byte[] sharedSecret = KeyExchange.GetSharedSecret(senderPrivateKey, recipientPublicKey);
        CryptographicOperations.ZeroMemory(senderPrivateKey);
        foreach (string inputFilePath in filePaths)
        {
            UsingPublicKey(inputFilePath, sharedSecret, recipientPublicKey);
        }
        CryptographicOperations.ZeroMemory(sharedSecret);
        DisplayMessage.SuccessfullyEncrypted();
    }

    private static void UsingPublicKey(string inputFilePath, byte[] sharedSecret, byte[] recipientPublicKey)
    {
        try
        {
            if (FileHandling.IsDirectory(inputFilePath))
            {
                DirectoryEncryption.UsingPublicKey(inputFilePath, sharedSecret, recipientPublicKey);
                return;
            }
            byte[] ephemeralSharedSecret = KeyExchange.GetPublicKeySharedSecret(recipientPublicKey, out byte[] ephemeralPublicKey);
            byte[] salt = SodiumCore.GetRandomBytes(Constants.SaltLength);
            byte[] keyEncryptionKey = KeyDerivation.Blake2(sharedSecret, ephemeralSharedSecret, salt);
            EncryptInputFile(inputFilePath, ephemeralPublicKey, salt, keyEncryptionKey);
        }
        catch (Exception ex) when (ExceptionFilters.Cryptography(ex))
        {
            DisplayMessage.FilePathException(inputFilePath, ex.GetType().Name, ErrorMessages.UnableToEncryptFile);
        }
    }

    public static void EncryptEachFileWithPrivateKey(string[] filePaths, byte[] privateKey)
    {
        if (filePaths == null || privateKey == null) { return; }
        privateKey = PrivateKey.Decrypt(privateKey);
        if (privateKey == null) { return; }
        foreach (string inputFilePath in filePaths)
        {
            UsingPrivateKey(inputFilePath, privateKey);
        }
        CryptographicOperations.ZeroMemory(privateKey);
        DisplayMessage.SuccessfullyEncrypted();
    }

    private static void UsingPrivateKey(string inputFilePath, byte[] privateKey)
    {
        try
        {
            if (FileHandling.IsDirectory(inputFilePath))
            {
                DirectoryEncryption.UsingPrivateKey(inputFilePath, privateKey);
                return;
            }
            byte[] ephemeralSharedSecret = KeyExchange.GetPrivateKeySharedSecret(privateKey, out byte[] ephemeralPublicKey);
            byte[] salt = SodiumCore.GetRandomBytes(Constants.SaltLength);
            byte[] keyEncryptionKey = KeyDerivation.Blake2(ephemeralSharedSecret, salt);
            EncryptInputFile(inputFilePath, ephemeralPublicKey, salt, keyEncryptionKey);
        }
        catch (Exception ex) when (ExceptionFilters.Cryptography(ex))
        {
            DisplayMessage.FilePathException(inputFilePath, ex.GetType().Name, ErrorMessages.UnableToEncryptFile);
        }
    }
}