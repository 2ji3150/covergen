namespace covergen {
    public class Config {
        public ushort? Noise { get; set; }
        public Crop Crop { get; set; }
    }

    public class Crop {
        public ushort Top { get; set; }
        public ushort Bottom { get; set; }
        public ushort Left { get; set; }
        public ushort Right { get; set; }
    }
}