[![License: GPLv3](https://img.shields.io/badge/License-GPLv3-blue.svg)](http://www.gnu.org/licenses/gpl-3.0)
[![CodeQL](https://github.com/samuel-lucas6/Kryptor/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/samuel-lucas6/Kryptor/actions)
[![Specification](https://img.shields.io/badge/%23-specification-blueviolet)](https://www.kryptor.co.uk/specification)

# Kryptor

Kryptor is a simple, modern, and secure file encryption and signing tool for Windows, Linux, and macOS.

It aims to be a better version of [age](https://github.com/FiloSottile/age) and [Minisign](https://jedisct1.github.io/minisign/) to provide a leaner, user friendly alternative to [GPG](https://gnupg.org/).

![kryptor](https://github.com/samuel-lucas6/Kryptor/assets/63159663/51f49cb7-f06e-4928-9eb6-cb7176ce4706)

## Features

- The latest and greatest cryptographic primitives, with no config options.
- Encrypt multiple files/directories with a passphrase, symmetric key, or asymmetric keys.
- Encrypt to multiple recipients for sender authenticated, one-way file sharing.
- Encrypted files are indistinguishable from random. File names can also be encrypted.
- Create and verify digital signatures, with support for an authenticated comment and prehashing.
- Small public keys. Private keys are encrypted for protection at rest.
- UNIX style passphrase entry and random passphrase generation.
- Pre-shared keys can be used for post-quantum secure key exchange.

For more information, please go to [kryptor.co.uk](https://www.kryptor.co.uk/).

## Usage

If you're just getting started, check out the [tutorial](https://www.kryptor.co.uk/tutorial) instead.

```
Usage: kryptor [options] <file>

Arguments:
  file             specify a file/directory path

Options:
  -e|--encrypt     encrypt files/directories
  -d|--decrypt     decrypt files/directories
  -p|--passphrase  specify a passphrase (empty for interactive entry)
  -k|--key         specify or randomly generate a symmetric key or keyfile
  -x|--private     specify your private key (unused or empty for default key)
  -y|--public      specify a public key
  -n|--names       encrypt file/directory names
  -o|--overwrite   overwrite files
  -g|--generate    generate a new key pair
  -r|--recover     recover your public key from your private key
  -m|--modify      change your private key passphrase
  -s|--sign        create a signature
  -c|--comment     add a comment to a signature or new key pair
  -l|--prehash     sign large files by prehashing
  -v|--verify      verify a signature
  -t|--signature   specify a signature file (unused for default name)
  --version        view the program version
  -h|--help        show help information

Examples:
  --encrypt [file]
  --encrypt -p [file]
  --encrypt [-y recipient's public key] [file]
  --decrypt [-y sender's public key] [file]
  --sign [-c comment] [file]
  --verify [-y public key] [file]
```

### Specifying files

When referencing file names/paths that contain spaces, you must surround them with "speech marks":

```
$ kryptor -e -p "GitHub Logo.png"
$ kryptor -e -p "C:\Users\samuel-lucas6\Downloads\GitHub Logo.png"
```

Files in the same directory as the `kryptor` executable can be specified using their file name:

```
$ kryptor -e -p file.txt
```

However, files that aren't in the same directory must be specified using a file path:

```
$ kryptor -e -p C:\Users\samuel-lucas6\Documents\file.txt
```

Multiple files and/or directories can be specified at once:

```
$ kryptor -e file1.txt file2.jpg file3.mp4 Photos Videos
```

### Specifying your private key

You can perform encryption, decryption, and signing with your default private key as follows:

```
$ kryptor -e file.txt
$ kryptor -d file.txt.bin
$ kryptor -s file.txt
```

This is the recommended approach, but it means your private keys must be kept in the default folder, which varies depending on your operating system:

- Windows: `%USERPROFILE%/.kryptor`
- Linux: `/home/.kryptor`
- macOS: `/Users/USERNAME/.kryptor`

To specify a private key for `-r|--recover`, `-m|--modify`, or a private key not stored in the default folder, you must use the `-x|--private` option followed by `:[file]` like so:

```
$ kryptor -r -x:"C:\Users\samuel-lucas6\Documents\encryption.private"
```
