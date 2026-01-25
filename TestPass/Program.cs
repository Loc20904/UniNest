namespace PasswordHasherTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== TOOL TẠO HASH PASSWORD (BCrypt) ===");
            Console.WriteLine("---------------------------------------");

            while (true)
            {
                Console.Write("\nNhập mật khẩu cần Hash (gõ 'exit' để thoát): ");
                string password = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(password)) continue;
                if (password.ToLower() == "exit") break;

                // 1. Tạo Hash
                // BCrypt tự động tạo Salt, nên mỗi lần chạy Hash sẽ khác nhau
                // nhưng vẫn verify đúng.
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n> Hash Output (Copy dòng dưới vào DB):");
                Console.ResetColor();
                Console.WriteLine(passwordHash);

                // 2. Test thử Verify (để chắc chắn nó hoạt động)
                Console.WriteLine("\n[Test] Đang thử kiểm tra lại...");
                bool isValid = BCrypt.Net.BCrypt.Verify(password, passwordHash);

                if (isValid)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("✅ Verify thành công! Hash này hợp lệ.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Verify thất bại.");
                }
                Console.ResetColor();
                Console.WriteLine("---------------------------------------");
            }
        }
    }
}