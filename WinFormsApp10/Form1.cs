using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace WinFormsApp10
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async Task<string> EncryptNewdesAsync(string input)
        {
            await Task.Delay(1000);

            byte[] bytesToEncrypt = Encoding.UTF8.GetBytes(input);
            using (var des = DES.Create())
            {
                des.Key = Encoding.UTF8.GetBytes("12345678");
                des.IV = Encoding.UTF8.GetBytes("87654321");
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToEncrypt, 0, bytesToEncrypt.Length);
                        cs.FlushFinalBlock();
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        private async Task<string> DecryptNewdesAsync(string encryptedInput)
        {
            await Task.Delay(1000);
            byte[] bytesToDecrypt = Convert.FromBase64String(encryptedInput);
            using (var des = DES.Create())
            {
                des.Key = Encoding.UTF8.GetBytes("12345678");
                des.IV = Encoding.UTF8.GetBytes("87654321");
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToDecrypt, 0, bytesToDecrypt.Length);
                        cs.FlushFinalBlock();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        private async Task<string> HashMd5Async(string input)
        {
            await Task.Delay(1000);

            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public class ElGamalService
        {
            public async Task<string> EncryptAndDecryptAsync(string input, int keySize)
            {
                await Task.Delay(1000);

                try
                {
                    ElGamalParametersGenerator parGen = new ElGamalParametersGenerator();
                    parGen.Init(keySize, 10, new SecureRandom());
                    ElGamalParameters elParams = parGen.GenerateParameters();

                    ElGamalKeyPairGenerator pGen = new ElGamalKeyPairGenerator();
                    pGen.Init(new ElGamalKeyGenerationParameters(new SecureRandom(), elParams));
                    AsymmetricCipherKeyPair pair = pGen.GenerateKeyPair();

                    ElGamalEngine cryptEng = new ElGamalEngine();
                    cryptEng.Init(true, pair.Public);
                    byte[] data = Encoding.ASCII.GetBytes(input);
                    byte[] enc = cryptEng.ProcessBlock(data, 0, data.Length);

                    ElGamalEngine decryptEng = new ElGamalEngine();
                    decryptEng.Init(false, pair.Private);
                    byte[] plain = decryptEng.ProcessBlock(enc, 0, enc.Length);

                    return $"Вхідні дані: {input}\nШифрування: {Convert.ToBase64String(enc)}\nДешифрування: {Encoding.ASCII.GetString(plain)}";
                }
                catch (Exception ex)
                {
                    return $"An error occurred: {ex.Message}";
                }
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            ElGamalService elGamalService = new ElGamalService();
            string input = "Hello";
            int keySize = 256;
            string result = await elGamalService.EncryptAndDecryptAsync(input, keySize);
            richTextBox3.AppendText(result + "\n");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string input = "Тестовий вхід для шифрування NewDES.";
            string encryptedResult = await EncryptNewdesAsync(input);
            richTextBox1.AppendText($"Результат шифрування NewDES: {encryptedResult}\n");

            string decryptedResult = await DecryptNewdesAsync(encryptedResult);
            richTextBox1.AppendText($"Результат дешифрування NewDES: {decryptedResult}\n");
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            string input = "Тестовий вхід для хешування MD5\r\n.";
            string result = await HashMd5Async(input);
            richTextBox2.AppendText($"Результат хешування MD5: {result}\n");
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            string inputNewDes = "Тестовий вхід для шифрування NewDES.";
            string inputMd5 = "Тестовий вхід для хешування MD5.";
            string inputElGamal = "Hello";
            int elGamalKeySize = 256;

            Task<string> taskNewDes = EncryptNewdesAsync(inputNewDes);
            Task<string> taskMd5 = HashMd5Async(inputMd5);
            Task<string> taskElGamal = new ElGamalService().EncryptAndDecryptAsync(inputElGamal, elGamalKeySize);

            await Task.WhenAll(taskNewDes, taskMd5, taskElGamal);

            string resultNewDes = await taskNewDes;
            string resultMd5 = await taskMd5;
            string resultElGamal = await taskElGamal;

            richTextBox1.AppendText($"Результат шифрування NewDES: {resultNewDes}\n");
            richTextBox2.AppendText($"Результат хешування MD5: {resultMd5}\n");
            richTextBox3.AppendText($"{resultElGamal}\n");
        }
    }
}
