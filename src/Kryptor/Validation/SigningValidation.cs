﻿/*
    Kryptor: A simple, modern, and secure encryption and signing tool.
    Copyright (C) 2020-2023 Samuel Lucas

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
using System.IO;
using System.Collections.Generic;

namespace Kryptor;

public static class SigningValidation
{
    private const string WrongSignatureExtension = "This isn't a .signature file.";
    private const string SignatureFileInaccessible = "Unable to access the signature file.";

    public static IEnumerable<string> GetSignErrors(string privateKeyPath, string comment, string[] signaturePaths, string[] filePaths)
    {
        if (string.Equals(privateKeyPath, Constants.DefaultSigningPrivateKeyPath) && !File.Exists(Constants.DefaultSigningPrivateKeyPath)) {
            yield return ErrorMessages.NonExistentDefaultPrivateKeyFile;
        }
        else switch (string.IsNullOrEmpty(privateKeyPath)) {
            case false when !privateKeyPath.EndsWith(Constants.PrivateKeyExtension):
                yield return ErrorMessages.GetFilePathError(privateKeyPath, ErrorMessages.InvalidPrivateKeyFile);
                break;
            case false when !File.Exists(privateKeyPath):
                yield return ErrorMessages.GetFilePathError(privateKeyPath, ErrorMessages.NonExistentPrivateKeyFile);
                break;
            case false when new FileInfo(privateKeyPath).Length < Constants.SigningPrivateKeyLength:
                yield return ErrorMessages.GetFilePathError(privateKeyPath, ErrorMessages.InvalidPrivateKeyFileLength);
                break;
        }

        if (!string.IsNullOrEmpty(comment) && comment.Length > Constants.MaxCommentLength) {
            yield return ErrorMessages.InvalidCommentLength;
        }

        if (signaturePaths != null) {
            foreach (string signaturePath in signaturePaths) {
                if (Directory.Exists(signaturePath)) {
                    yield return ErrorMessages.GetFilePathError(signaturePath, "This is a directory, not a signature file.");
                }
                else if (!signaturePath.EndsWith(Constants.SignatureExtension)) {
                    yield return ErrorMessages.GetFilePathError(signaturePath, WrongSignatureExtension);
                }
            }
        }

        if (filePaths == null) {
            yield return "Specify a file/directory to sign.";
        }
        else {
            foreach (string filePath in filePaths) {
                if (Directory.Exists(filePath)) {
                    bool? isEmpty = FileHandling.IsDirectoryEmpty(filePath);
                    if (isEmpty == true) {
                        yield return ErrorMessages.GetFilePathError(filePath, ErrorMessages.DirectoryEmpty);
                    }
                    else if (isEmpty == null) {
                        yield return ErrorMessages.GetFilePathError(filePath, ErrorMessages.UnableToAccessDirectory);
                    }
                    if (signaturePaths != null) {
                        yield return ErrorMessages.GetFilePathError(filePath, "Signature files cannot be specified when signing a directory.");
                    }
                }
                else if (!File.Exists(filePath)) {
                    yield return ErrorMessages.GetFilePathError(filePath, ErrorMessages.FileOrDirectoryDoesNotExist);
                }
            }
        }
        if (signaturePaths != null && filePaths != null && signaturePaths.Length != filePaths.Length) {
            yield return "Specify the same number of signature files and files to sign.";
        }
    }

    public static IEnumerable<string> GetVerifyErrors(string[] publicKeys, string[] signaturePaths, string[] filePaths)
    {
        if (publicKeys == null) {
            yield return ErrorMessages.NoPublicKey;
        }
        else if (publicKeys.Length > 1) {
            yield return ErrorMessages.MultiplePublicKeys;
        }
        else if (!publicKeys[0].EndsWith(Constants.PublicKeyExtension)) {
            if (File.Exists(publicKeys[0]) || !string.IsNullOrEmpty(Path.GetExtension(publicKeys[0]))) {
                yield return ErrorMessages.GetFilePathError(publicKeys[0], ErrorMessages.InvalidPublicKeyFile);
            }
            else if (string.IsNullOrEmpty(Path.GetExtension(publicKeys[0])) && publicKeys[0].Length != Constants.PublicKeyLength) {
                yield return ErrorMessages.GetKeyStringError(publicKeys[0], ErrorMessages.InvalidPublicKey);
            }
        }
        else if (!File.Exists(publicKeys[0])) {
            yield return ErrorMessages.GetFilePathError(publicKeys[0], ErrorMessages.NonExistentPublicKeyFile);
        }
        else if (new FileInfo(publicKeys[0]).Length < Constants.PublicKeyLength) {
            yield return ErrorMessages.GetFilePathError(publicKeys[0], ErrorMessages.InvalidPublicKeyFileLength);
        }

        if (filePaths == null) {
            yield return "Specify a file to verify.";
        }
        else {
            foreach (string filePath in filePaths) {
                string errorMessage = GetVerifyFileError(filePath, signaturePaths == null ? filePath + Constants.SignatureExtension : string.Empty);
                if (!string.IsNullOrEmpty(errorMessage)) {
                    yield return ErrorMessages.GetFilePathError(filePath, errorMessage);
                }
            }
        }

        if (signaturePaths == null) {
            yield break;
        }
        foreach (string signaturePath in signaturePaths) {
            string errorMessage = GetSignatureFileError(signaturePath);
            if (!string.IsNullOrEmpty(errorMessage)) {
                yield return ErrorMessages.GetFilePathError(signaturePath, errorMessage);
            }
        }
        if (filePaths != null && signaturePaths.Length != filePaths.Length) {
            yield return "Specify the same number of signature files and files to verify.";
        }
    }

    private static string GetVerifyFileError(string filePath, string signatureFilePath)
    {
        if (filePath.EndsWith(Constants.SignatureExtension)) { return "Specify the file to verify, not the signature file."; }
        if (Directory.Exists(filePath)) { return "Only specify files when verifying, not directories."; }
        if (!File.Exists(filePath)) { return "This file doesn't exist."; }
        if (string.IsNullOrEmpty(signatureFilePath)) { return null; }
        if (!File.Exists(signatureFilePath)) { return "Unable to find the signature file. Specify it manually using -t|--signature."; }
        bool? validMagicBytes = IsValidSignatureFile(signatureFilePath, out bool? validVersion);
        if (validMagicBytes == null) { return SignatureFileInaccessible; }
        if (validMagicBytes == false) { return "The signature file that was found doesn't have a valid format."; }
        return validVersion == false ? "The signature file that was found doesn't have a valid version." : null;
    }

    private static string GetSignatureFileError(string signatureFilePath)
    {
        if (!signatureFilePath.EndsWith(Constants.SignatureExtension)) { return WrongSignatureExtension; }
        if (!File.Exists(signatureFilePath)) { return "This signature file doesn't exist."; }
        bool? validMagicBytes = IsValidSignatureFile(signatureFilePath, out bool? validVersion);
        if (validMagicBytes == null) { return SignatureFileInaccessible; }
        if (validMagicBytes == false) { return "Invalid signature file format."; }
        return validVersion == false ? "Invalid signature file version." : null;
    }

    private static bool? IsValidSignatureFile(string filePath, out bool? validVersion)
    {
        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 0);
            Span<byte> magicBytes = stackalloc byte[Constants.SignatureMagicBytes.Length];
            fileStream.Read(magicBytes);
            Span<byte> version = stackalloc byte[Constants.SignatureVersion.Length];
            fileStream.Read(version);
            validVersion = version.SequenceEqual(Constants.SignatureVersion);
            return magicBytes.SequenceEqual(Constants.SignatureMagicBytes);
        }
        catch (Exception ex) when (ExceptionFilters.FileAccess(ex))
        {
            validVersion = null;
            return null;
        }
    }
}
