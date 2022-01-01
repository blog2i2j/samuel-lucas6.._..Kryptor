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
using System.Net;
using System.Security.Cryptography;

namespace KryptorCLI;

public static class CommandLine
{
    public static void Encrypt(bool usePassword, string keyfile, string privateKey, string publicKey, string[] filePaths)
    {
        if (usePassword || !string.IsNullOrEmpty(keyfile))
        {
            FileEncryptionWithPassword(usePassword, keyfile, filePaths);
        }
        else if (!string.IsNullOrEmpty(publicKey) && !string.IsNullOrEmpty(privateKey))
        {
            if (publicKey.EndsWith(Constants.PublicKeyExtension))
            {
                FileEncryptionWithPublicKey(privateKey, publicKey, filePaths);
                return;
            }
            FileEncryptionWithPublicKey(privateKey, publicKey.ToCharArray(), filePaths);
        }
        else if (!string.IsNullOrEmpty(privateKey))
        {
            FileEncryptionWithPrivateKey(privateKey, filePaths);
        }
        else
        {
            DisplayMessage.Error(ErrorMessages.PasswordBasedEncryption);
        }
    }

    private static void FileEncryptionWithPassword(bool usePassword, string keyfilePath, string[] filePaths)
    {
        bool validUserInput = FileEncryptionValidation.FileEncryptionWithPassword(usePassword, keyfilePath, filePaths);
        if (!validUserInput) { return; }
        char[] password = Array.Empty<char>();
        if (usePassword) { password = PasswordPrompt.EnterNewPassword(); }
        var passwordBytes = Password.Prehash(password, FilePathValidation.KeyfilePath(keyfilePath));
        FileEncryption.EncryptEachFileWithPassword(filePaths, passwordBytes);
    }

    private static void FileEncryptionWithPublicKey(string senderPrivateKeyPath, string recipientPublicKeyPath, string[] filePaths)
    {
        bool validUserInput = FileEncryptionValidation.FileEncryptionWithPublicKey(senderPrivateKeyPath, recipientPublicKeyPath, filePaths);
        if (!validUserInput) { return; }
        byte[] senderPrivateKey = AsymmetricKeyValidation.EncryptionPrivateKeyFile(senderPrivateKeyPath);
        byte[] recipientPublicKey = AsymmetricKeyValidation.EncryptionPublicKeyFile(recipientPublicKeyPath);
        FileEncryption.EncryptEachFileWithPublicKey(filePaths, senderPrivateKey, recipientPublicKey);
    }

    private static void FileEncryptionWithPublicKey(string senderPrivateKeyPath, char[] recipientPublicKeyString, string[] filePaths)
    {
        bool validUserInput = FileEncryptionValidation.FileEncryptionWithPublicKey(senderPrivateKeyPath, recipientPublicKeyString, filePaths);
        if (!validUserInput) { return; }
        byte[] senderPrivateKey = AsymmetricKeyValidation.EncryptionPrivateKeyFile(senderPrivateKeyPath);
        byte[] recipientPublicKey = AsymmetricKeyValidation.EncryptionPublicKeyString(recipientPublicKeyString);
        FileEncryption.EncryptEachFileWithPublicKey(filePaths, senderPrivateKey, recipientPublicKey);
    }

    private static void FileEncryptionWithPrivateKey(string privateKeyPath, string[] filePaths)
    {
        bool validUserInput = FileEncryptionValidation.FileEncryptionWithPrivateKey(privateKeyPath, filePaths);
        if (!validUserInput) { return; }
        byte[] privateKey = AsymmetricKeyValidation.EncryptionPrivateKeyFile(privateKeyPath);
        FileEncryption.EncryptEachFileWithPrivateKey(filePaths, privateKey);
    }

    public static void Decrypt(bool usePassword, string keyfile, string privateKey, string publicKey, string[] filePaths)
    {
        if (usePassword || !string.IsNullOrEmpty(keyfile))
        {
            FileDecryptionWithPassword(usePassword, keyfile, filePaths);
        }
        else if (!string.IsNullOrEmpty(publicKey) && !string.IsNullOrEmpty(privateKey))
        {
            if (publicKey.EndsWith(Constants.PublicKeyExtension))
            {
                FileDecryptionWithPublicKey(privateKey, publicKey, filePaths);
                return;
            }
            FileDecryptionWithPublicKey(privateKey, publicKey.ToCharArray(), filePaths);
        }
        else if (!string.IsNullOrEmpty(privateKey))
        {
            FileDecryptionWithPrivateKey(privateKey, filePaths);
        }
        else
        {
            DisplayMessage.Error(ErrorMessages.PasswordBasedEncryption);
        }
    }

    private static void FileDecryptionWithPassword(bool usePassword, string keyfilePath, string[] filePaths)
    {
        bool validUserInput = FileEncryptionValidation.FileDecryptionWithPassword(usePassword, keyfilePath, filePaths);
        if (!validUserInput) { return; }
        char[] password = Array.Empty<char>();
        if (usePassword) { password = PasswordPrompt.EnterYourPassword(); }
        var passwordBytes = Password.Prehash(password, keyfilePath);
        FileDecryption.DecryptEachFileWithPassword(filePaths, passwordBytes);
    }

    private static void FileDecryptionWithPublicKey(string recipientPrivateKeyPath, string senderPublicKeyPath, string[] filePaths)
    {
        bool validUserInput = FileEncryptionValidation.FileDecryptionWithPublicKey(recipientPrivateKeyPath, senderPublicKeyPath, filePaths);
        if (!validUserInput) { return; }
        byte[] recipientPrivateKey = AsymmetricKeyValidation.EncryptionPrivateKeyFile(recipientPrivateKeyPath);
        byte[] senderPublicKey = AsymmetricKeyValidation.EncryptionPublicKeyFile(senderPublicKeyPath);
        FileDecryption.DecryptEachFileWithPublicKey(filePaths, recipientPrivateKey, senderPublicKey);
    }

    private static void FileDecryptionWithPublicKey(string recipientPrivateKeyPath, char[] senderPublicKeyString, string[] filePaths)
    {
        bool validUserInput = FileEncryptionValidation.FileDecryptionWithPublicKey(recipientPrivateKeyPath, senderPublicKeyString, filePaths);
        if (!validUserInput) { return; }
        byte[] recipientPrivateKey = AsymmetricKeyValidation.EncryptionPrivateKeyFile(recipientPrivateKeyPath);
        byte[] senderPublicKey = AsymmetricKeyValidation.EncryptionPublicKeyString(senderPublicKeyString);
        FileDecryption.DecryptEachFileWithPublicKey(filePaths, recipientPrivateKey, senderPublicKey);
    }

    private static void FileDecryptionWithPrivateKey(string privateKeyPath, string[] filePaths)
    {
        bool validUserInput = FileEncryptionValidation.FileDecryptionWithPrivateKey(privateKeyPath, filePaths);
        if (!validUserInput) { return; }
        byte[] privateKey = AsymmetricKeyValidation.EncryptionPrivateKeyFile(privateKeyPath);
        FileDecryption.DecryptEachFileWithPrivateKey(filePaths, privateKey);
    }

    public static void GenerateNewKeyPair(string exportDirectoryPath)
    {
        try
        {
            int keyPairType = GetKeyPairType();
            bool validUserInput = FilePathValidation.GenerateKeyPair(exportDirectoryPath, keyPairType);
            if (!validUserInput) { return; }
            string publicKey, privateKey, publicKeyFilePath, privateKeyFilePath;
            if (keyPairType == 1)
            {
                (publicKey, privateKey) = AsymmetricKeys.GenerateEncryptionKeyPair();
                (publicKeyFilePath, privateKeyFilePath) = AsymmetricKeys.ExportKeyPair(exportDirectoryPath, Constants.DefaultEncryptionKeyFileName, publicKey, privateKey);
            }
            else
            {
                (publicKey, privateKey) = AsymmetricKeys.GenerateSigningKeyPair();
                (publicKeyFilePath, privateKeyFilePath) = AsymmetricKeys.ExportKeyPair(exportDirectoryPath, Constants.DefaultSigningKeyFileName, publicKey, privateKey);
            }
            DisplayMessage.KeyPair(publicKey, publicKeyFilePath, privateKeyFilePath);
        }
        catch (Exception ex) when (ExceptionFilters.FileAccess(ex))
        {
            DisplayMessage.FilePathException(exportDirectoryPath, ex.GetType().Name, "Unable to create the key pair files.");
        }
    }

    private static int GetKeyPairType()
    {
        Console.WriteLine("Please select a key pair type (type 1 or 2):");
        Console.WriteLine("1) Encryption");
        Console.WriteLine("2) Signing");
        string userInput = Console.ReadLine();
        Console.WriteLine();
        _ = int.TryParse(userInput, out int keyPairType);
        return keyPairType;
    }

    public static void RecoverPublicKey(string privateKeyFilePath)
    {
        bool validUserInput = FilePathValidation.RecoverPublicKey(privateKeyFilePath);
        if (!validUserInput) { return; }
        byte[] privateKey = AsymmetricKeyValidation.GetPrivateKeyFromFile(privateKeyFilePath);
        privateKey = PrivateKey.Decrypt(privateKey);
        if (privateKey == null) { return; }
        byte[] publicKey = privateKey.Length switch
        {
            Constants.EncryptionKeyLength => AsymmetricKeys.GetCurve25519PublicKey(privateKey),
            _ => AsymmetricKeys.GetEd25519PublicKey(privateKey)
        };
        string publicKeyString = Convert.ToBase64String(publicKey);
        string publicKeyFilePath = AsymmetricKeys.ExportPublicKey(privateKeyFilePath, publicKeyString);
        DisplayMessage.PublicKey(publicKeyString, publicKeyFilePath);
    }

    public static void Sign(string privateKeyPath, string comment, bool preHash, string signatureFilePath, string[] filePaths)
    {
        bool validUserInput = SigningValidation.Sign(privateKeyPath, comment, signatureFilePath, filePaths);
        if (!validUserInput) { return; }
        byte[] privateKey = AsymmetricKeyValidation.SigningPrivateKeyFile(privateKeyPath);
        FileSigning.SignEachFile(filePaths, signatureFilePath, comment, preHash, privateKey);
    }

    public static void Verify(string publicKey, string signatureFilePath, string[] filePaths)
    {
        if (string.IsNullOrEmpty(publicKey) || publicKey.EndsWith(Constants.PublicKeyExtension))
        {
            VerifySignature(publicKey, signatureFilePath, filePaths);
            return;
        }
        VerifySignature(publicKey.ToCharArray(), signatureFilePath, filePaths);
    }

    private static void VerifySignature(char[] encodedPublicKey, string signatureFilePath, string[] filePaths)
    {
        signatureFilePath = FilePathValidation.GetSignatureFilePath(signatureFilePath, filePaths);
        bool validUserInput = SigningValidation.Verify(encodedPublicKey, signatureFilePath, filePaths);
        if (!validUserInput) { return; }
        byte[] publicKey = AsymmetricKeyValidation.SigningPublicKeyString(encodedPublicKey);
        FileSigning.VerifyFile(signatureFilePath, filePaths[0], publicKey);
    }

    private static void VerifySignature(string publicKeyPath, string signatureFilePath, string[] filePaths)
    {
        signatureFilePath = FilePathValidation.GetSignatureFilePath(signatureFilePath, filePaths);
        bool validUserInput = SigningValidation.Verify(publicKeyPath, signatureFilePath, filePaths);
        if (!validUserInput) { return; }
        byte[] publicKey = AsymmetricKeyValidation.SigningPublicKeyFile(publicKeyPath);
        FileSigning.VerifyFile(signatureFilePath, filePaths[0], publicKey);
    }
    
    public static void CheckForUpdates()
    {
        try
        {
            bool updateAvailable = Updates.CheckForUpdates(out string latestVersion);
            if (!updateAvailable)
            {
                Console.WriteLine("Kryptor is up-to-date.");
                return;
            }
            Console.WriteLine("An update is available for Kryptor.");
            Console.WriteLine();
            Console.WriteLine($"IMPORTANT: Please check the latest changelog at <https://www.kryptor.co.uk/changelog#v{latestVersion}> to see if there are any breaking changes BEFORE updating.");
            Console.WriteLine();
            Console.WriteLine("Would you like Kryptor to automatically install this update now? (type y or n)");
            string userInput = Console.ReadLine()?.ToLower();
            if (!string.IsNullOrEmpty(userInput)) { Console.WriteLine(); }
            switch (userInput)
            {
                case "y":
                    Updates.Update(latestVersion);
                    break;
                case "n":
                    Console.WriteLine("Please specify -u|--update again when you're ready to update.");
                    Console.WriteLine();
                    Console.WriteLine("Alternatively, you can manually download the latest release at <https://www.kryptor.co.uk/#download-kryptor>.");
                    return;
                default:
                    Console.WriteLine("Please type either y or n next time.");
                    return;
            }
        }
        catch (Exception ex) when (ex is WebException or PlatformNotSupportedException or FormatException or OverflowException || ExceptionFilters.Cryptography(ex))
        {
            if (ex is PlatformNotSupportedException or CryptographicException)
            {
                DisplayMessage.Exception(ex.GetType().Name, ex.Message);
                return;
            }
            DisplayMessage.Exception(ex.GetType().Name, "Unable to check for or download updates.");
        }
    }

    public static void DisplayAbout()
    {
        Console.WriteLine($"Kryptor v{Program.GetVersion()}");
        Console.WriteLine("Copyright (C) 2020-2022 Samuel Lucas");
        Console.WriteLine("License GPLv3+: GNU GPL version 3 or later <https://www.gnu.org/licenses/gpl-3.0.html>.");
        Console.WriteLine("This is free software: you are free to change and redistribute it.");
        Console.WriteLine("There is NO WARRANTY, to the extent permitted by law.");
    }
}