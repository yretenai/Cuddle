using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using DragonLib;

namespace Cuddle.Core;

public class FArchiveWriter {
    public FArchiveWriter() {
        Buffer = new byte[0x1000].AsMemory();
    }
    
    private Memory<byte> Buffer { get; set; }
    public ReadOnlyMemory<byte> Data => Buffer[..Size]; 
    public int Size { get; private set; }
    public int Position { get; set; }

    private void GrowDataIfNecessary(int size) {
        var endPos = Position + size;
        if (endPos > Size) {
            Size = endPos;
        }
        
        if (endPos < Buffer.Length) {
            return;
        }
        
        var overflow = (endPos - Buffer.Length).Align(0x1000);
        var newData = new byte[Buffer.Length + overflow].AsMemory();
        Buffer.CopyTo(newData);
        Buffer = newData;
    }
    
    
    public void Write<T>(T value) where T : unmanaged {
        var size = Unsafe.SizeOf<T>();
        GrowDataIfNecessary(size);
        MemoryMarshal.Write(Buffer.Span[Position..], ref value);
        Position += size;
    }

    public void Write(bool truth) {
        Write(truth ? 1 : 0);
    }
    
    public void Write(string str, int? strSize = null, bool wide = false) {
        var strSizeWasNull = strSize == null;
        strSize ??= str.Length;
        Span<byte> data;
        if (wide) {
            strSize = -strSize;
            data = Encoding.Unicode.GetBytes(str);
            if (strSizeWasNull) {
                strSize += 2;
            }
        } else {
            data = Encoding.UTF8.GetBytes(str);
            if (strSizeWasNull) {
                strSize += 2;
            }
        }

        if (strSizeWasNull) {
            Write(strSize);
        }

        GrowDataIfNecessary(strSize.Value);
        data[..strSize.Value].CopyTo(Buffer.Span[Position..]);
        Position += strSize.Value;
    }

    public void Write(int? strSize = null, bool wide = false, params string[] strings) {
        Write(strings.Length);

        foreach (var @string in strings)
        {
            Write(@string, strSize, wide);
        }
    }

    public void Write<T>(IEnumerable<T> value) where T : unmanaged {
        Write(value is T[] arr ? arr.AsSpan() : value.ToArray().AsSpan());
    }

    public void Write<T>(Memory<T> value) where T : unmanaged {
        Write(value.Span);
    }

    public void Write<T>(Span<T> memory) where T : unmanaged {
        Write(memory.Length);
        
        var bytes = MemoryMarshal.AsBytes(memory);
        GrowDataIfNecessary(bytes.Length);
        Buffer.Span.Slice(Position);
        Position += bytes.Length;
    }

    public void Align(int alignment) {
        Position = Position.Align(alignment);
        if (Position > Size) {
            GrowDataIfNecessary(Position - Size);
        }
    }
}
