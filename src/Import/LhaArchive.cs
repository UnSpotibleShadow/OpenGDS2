using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenGDS2.Import;

/// <summary>
/// Minimal level-2 LHA reader supporting -lh0-, -lh5- and -lhd- entries.
/// Written specifically for importing the legally obtained GameDev2DX archive;
/// no original game code is executed.
/// </summary>
public sealed class LhaArchive
{
    private const int DicBit = 13;
    private const int Np = DicBit + 1;
    private const int Nt = 16 + 3;
    private const int PBit = 4;
    private const int TBit = 5;
    private const int MaxMatch = 256;
    private const int Threshold = 3;
    private const int Nc = 255 + MaxMatch + 2 - Threshold;
    private const int CBit = 9;

    public sealed record Entry(string Name, string Method, int PackedSize, int OriginalSize, int DataOffset, bool IsDirectory, ushort Crc16);

    private readonly byte[] _raw;
    public IReadOnlyList<Entry> Entries { get; }

    public LhaArchive(string path)
    {
        _raw = File.ReadAllBytes(path);
        Entries = ParseEntries(_raw);
    }

    public byte[] Extract(Entry entry)
    {
        if (entry.IsDirectory) return Array.Empty<byte>();
        var compressed = _raw.AsSpan(entry.DataOffset, entry.PackedSize).ToArray();
        var result = entry.Method switch
        {
            "-lh0-" => compressed,
            "-lh5-" => DecompressLh5(compressed, entry.OriginalSize),
            _ => throw new NotSupportedException($"Unsupported LHA method: {entry.Method}")
        };
        if (result.Length != entry.OriginalSize || ComputeCrc16(result) != entry.Crc16)
            throw new InvalidDataException($"LHA size or CRC mismatch: {entry.Name}");
        return result;
    }

    private static ushort ComputeCrc16(ReadOnlySpan<byte> bytes)
    {
        ushort crc = 0;
        foreach (byte value in bytes)
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++) crc = (ushort)((crc >> 1) ^ ((crc & 1) != 0 ? 0xA001 : 0));
        }
        return crc;
    }

    private static IReadOnlyList<Entry> ParseEntries(byte[] raw)
    {
        var result = new List<Entry>();
        int pos = 0;
        while (pos + 26 <= raw.Length)
        {
            ushort total = ReadU16(raw, pos);
            if (total == 0) break;
            if (total < 26 || total > raw.Length - pos) throw new InvalidDataException("Invalid LHA header size.");
            string method = Encoding.ASCII.GetString(raw, pos + 2, 5);
            int packed = checked((int)ReadU32(raw, pos + 7));
            int original = checked((int)ReadU32(raw, pos + 11));
            if (original > 64 * 1024 * 1024 || packed > raw.Length - pos - total)
                throw new InvalidDataException("Invalid LHA entry bounds.");
            byte level = raw[pos + 20];
            if (level != 2) throw new InvalidDataException($"Unsupported LHA header level {level} at {pos}.");

            ushort hs = ReadU16(raw, pos + 24);
            int q = pos + 26;
            byte[] name = Array.Empty<byte>();
            byte[] dir = Array.Empty<byte>();
            while (hs != 0)
            {
                ushort current = hs;
                if (q + current > pos + total || current < 3) throw new InvalidDataException("Corrupt LHA extended header.");
                byte type = raw[q];
                int payloadLen = current - 3;
                var payload = raw.AsSpan(q + 1, payloadLen).ToArray();
                if (type == 1) name = payload;
                else if (type == 2) dir = payload;
                hs = ReadU16(raw, q + current - 2);
                q += current;
            }

            string path = DecodePath(dir, name);
            int dataOffset = pos + total;
            bool directory = method == "-lhd-";
            result.Add(new Entry(path, method, packed, original, dataOffset, directory, ReadU16(raw, pos + 21)));
            pos = dataOffset + packed;
        }
        return result;
    }

    private static string DecodePath(byte[] dir, byte[] name)
    {
        var bytes = new byte[dir.Length + name.Length];
        Buffer.BlockCopy(dir, 0, bytes, 0, dir.Length);
        Buffer.BlockCopy(name, 0, bytes, dir.Length, name.Length);
        for (int i = 0; i < bytes.Length; i++) if (bytes[i] == 0xFF) bytes[i] = (byte)'/';
        // All filenames in this release are ASCII; CP932 is only needed for game text, not archive paths.
        return Encoding.ASCII.GetString(bytes).Replace('\\', '/').TrimEnd('/');
    }

    private static byte[] DecompressLh5(byte[] data, int outSize)
    {
        var bits = new BitReader(data);
        var output = new List<byte>(outSize);
        int block = 0;
        Huffman? ch = null, ph = null;
        while (output.Count < outSize)
        {
            if (block == 0)
            {
                block = bits.Get(16);
                var pth = ReadPtLen(bits, Nt, TBit, 3);
                ch = ReadCLen(bits, pth);
                ph = ReadPtLen(bits, Np, PBit, -1);
                if (block == 0) throw new InvalidDataException("Zero LH5 block.");
            }
            block--;
            int c = ch!.Decode(bits);
            if (c <= 255)
            {
                output.Add((byte)c);
                continue;
            }

            int length = c - (256 - Threshold);
            int j = ph!.Decode(bits);
            if (j != 0) j = (1 << (j - 1)) + bits.Get(j - 1);
            int distance = j + 1;
            for (int i = 0; i < length && output.Count < outSize; i++)
                output.Add(distance > output.Count ? (byte)0x20 : output[^distance]);
        }
        return output.ToArray();
    }

    private static Huffman ReadPtLen(BitReader bits, int nn, int nbit, int special)
    {
        int n = bits.Get(nbit);
        var lengths = new int[nn];
        if (n == 0) return Huffman.Constant(bits.Get(nbit));
        int i = 0;
        while (i < n && i < nn)
        {
            int c = bits.Get(3);
            if (c == 7) while (bits.Get(1) != 0) c++;
            lengths[i++] = c;
            if (i == special)
            {
                int zeros = bits.Get(2);
                while (zeros-- > 0 && i < nn) lengths[i++] = 0;
            }
        }
        return new Huffman(lengths);
    }

    private static Huffman ReadCLen(BitReader bits, Huffman pt)
    {
        int n = bits.Get(CBit);
        var lengths = new int[Nc];
        if (n == 0) return Huffman.Constant(bits.Get(CBit));
        int i = 0;
        while (i < n && i < Nc)
        {
            int c = pt.Decode(bits);
            if (c <= 2)
            {
                int zeros = c switch { 0 => 1, 1 => bits.Get(4) + 3, _ => bits.Get(CBit) + 20 };
                while (zeros-- > 0 && i < Nc) lengths[i++] = 0;
            }
            else lengths[i++] = c - 2;
        }
        return new Huffman(lengths);
    }

    private sealed class BitReader(byte[] data)
    {
        private int _position;
        public int Get(int count)
        {
            int value = 0;
            for (int i = 0; i < count; i++)
            {
                int bytePos = _position >> 3;
                int bit = bytePos >= data.Length ? 0 : (data[bytePos] >> (7 - (_position & 7))) & 1;
                value = (value << 1) | bit;
                _position++;
            }
            return value;
        }
    }

    private sealed class Huffman
    {
        private readonly int? _constant;
        private readonly Dictionary<(int Length, int Code), int> _map = new();
        private readonly int _maxLength;

        private Huffman(int constant) => _constant = constant;
        public static Huffman Constant(int symbol) => new(symbol);

        public Huffman(int[] lengths)
        {
            int max = 0;
            foreach (int l in lengths) max = Math.Max(max, l);
            _maxLength = max;
            var counts = new int[max + 1];
            foreach (int l in lengths) if (l != 0) counts[l]++;
            int code = 0;
            var next = new int[max + 1];
            for (int b = 1; b <= max; b++) { code = (code + counts[b - 1]) << 1; next[b] = code; }
            for (int symbol = 0; symbol < lengths.Length; symbol++)
            {
                int l = lengths[symbol];
                if (l == 0) continue;
                int c = next[l]++;
                _map[(l, c)] = symbol;
            }
        }

        public int Decode(BitReader bits)
        {
            if (_constant is int value) return value;
            int code = 0;
            for (int length = 1; length <= _maxLength; length++)
            {
                code = (code << 1) | bits.Get(1);
                if (_map.TryGetValue((length, code), out int symbol)) return symbol;
            }
            throw new InvalidDataException("Invalid LH5 Huffman code.");
        }
    }

    private static ushort ReadU16(byte[] data, int o) => (ushort)(data[o] | data[o + 1] << 8);
    private static uint ReadU32(byte[] data, int o) => (uint)(data[o] | data[o + 1] << 8 | data[o + 2] << 16 | data[o + 3] << 24);
}
