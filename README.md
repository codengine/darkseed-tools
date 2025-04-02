# Dark Seed Tools

Tools for Dark Seed.

## TosText

Reads and writes TOSTEXT.BIN.

#### Extract texts

Usage:
`TosText.exe extract -i "C:\DARKSEED\TOSTEXT.BIN" -o "C:\DARKSEED\TOSTEXT.TXT"`

```

  -i, --in     Required. Path to input (bin) file

  -o, --out    Required. Path to output (txt) file

  --mb         (Default: false) Toggle multibyte character encoding/decoding

  --help       Display this help screen.

  --version    Display version information.
```

#### Rebuild TOSTEXT.BIN

Usage:
`TosText.exe rebuild -i "C:\DARKSEED\TOSTEXT.TXT" -o "C:\DARKSEED\TOSTEXT.BIN"`

```
  -i, --in     Required. Path to input (txt) file

  -o, --out    Required. Path to output (bin) file

  --mb         (Default: false) Toggle multibyte character encoding/decoding

  --help       Display this help screen.

  --version    Display version information.
```


## TosSprite

Reads and writes sprites from and to .NSP files. An empty sprite is saved as a 1x1 transparent pixel.

Please take care about the color palette and disable dithering / antialiasing and so on that may lead to pixels that are not on the default palette.

#### Extract sprites

Usage:
`TosSprites.exe extract -i "C:\DARKSEED\CPLAYER.NSP" -o "C:\DARKSEED\out"`

```
  -i, --in     Required. Path to the input file

  -o, --out    Required. Path where the gif files are stored

  --help       Display this help screen.

  --version    Display version information.
```

#### Rebuild TOSTEXT.BIN

If the sprites are stored at CPLAYER_0.gif, CPLAYER_1.gif and so on, the prefix would be "CPLAYER".

Usage:
`TosSprites.exe rebuild -i "C:\DARKSEED\out" -o "C:\DARKSEED\CPLAYER.NSP" -o "CPLAYER"`

```
  -i, --in        Required. Path to input files

  -p, --prefix    Required. Input filename prefix

  -o, --out       Required. Path to output file
```
