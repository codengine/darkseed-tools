namespace TosSprites;

public class Sprite
{
    public ushort Height { get; }
    public ushort Width { get; }
    public ushort Pitch { get; }
    private readonly byte[] _pixels;
    public const byte TransparentPixel = 0xF;
    public bool IsEmptySprite { get; private set; }

    public Sprite(ushort width, ushort height, ushort pitch)
    {
        Height = height;
        Width = width;
        Pitch = pitch;
        _pixels = new byte[pitch * height];
        
        // Initialize pixels to transparent
        for (var i = 0; i < _pixels.Length; i++)
        {
            _pixels[i] = TransparentPixel;
        }
        
        IsEmptySprite = false;
    }

    public void Load(BinaryReader reader)
    {
        // Special handling for 1x1 sprites - check for empty (FF) sprite
        if (Width == 1 && Height == 1)
        {
            var value = reader.ReadByte();
            if (value == 0xFF)
            {
                IsEmptySprite = true;
                _pixels[0] = TransparentPixel;
            }
            else
            {
                _pixels[0] = (byte)(value >> 4);
            }
            return;
        }
        
        var hasReadByte = false;
        byte currentDataByte = 0;
        
        for (var i = 0; i < Pitch * Height; i++)
        {
            if (!hasReadByte)
            {
                currentDataByte = reader.ReadByte();
                hasReadByte = true;
                _pixels[i] = (byte)(currentDataByte >> 4);
            }
            else
            {
                hasReadByte = false;
                _pixels[i] = (byte)(currentDataByte & 0xF);
            }
        }
    }
    
    public void Save(BinaryWriter writer)
    {
        // Special case for empty sprites
        if (IsEmptySprite && Width == 1 && Height == 1)
        {
            writer.Write((byte)0xFF);
            return;
        }
        
        // Special case for 1x1 non-empty sprites
        if (Width == 1 && Height == 1 && !IsEmptySprite)
        {
            writer.Write((byte)(_pixels[0] << 4));
            return;
        }

        for (var y = 0; y < Height; y++)
        {
            var rowStart = y * Pitch;
            
            for (var x = 0; x < Pitch; x += 2)
            {
                var pixelIdx = rowStart + x;
                var highNibble = _pixels[pixelIdx];
                
                var lowNibble = (x + 1 < Pitch) ? _pixels[pixelIdx + 1] : TransparentPixel;
                writer.Write((byte)((highNibble << 4) | lowNibble));
            }
        }
    }
    
    public byte GetPixel(int x, int y)
    {
        if (x >= 0 && x < Pitch && y >= 0 && y < Height)
        {
            return _pixels[y * Pitch + x];
        }
        return 0;
    }
    
    public void SetPixel(int x, int y, byte value)
    {
        if (x >= 0 && x < Pitch && y >= 0 && y < Height)
        {
            _pixels[y * Pitch + x] = value;
        }
    }

    public static Sprite CreateEmpty()
    {
        return new Sprite(1, 1, 1)
        {
            IsEmptySprite = true
        };
    }
}