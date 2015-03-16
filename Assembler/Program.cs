using System;
using System.Linq;
using System.Collections.Generic;

namespace Assembler
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			try {
				var assembler = new Chip8Assembler();
				System.IO.FileInfo fi = new System.IO.FileInfo(args[0]);
				var outname = fi.Name.Replace(fi.Extension, ".ch8");

				var file = System.IO.File.OpenText(args[0]);
				string source = file.ReadToEnd();
				file.Close();

				byte[] output = assembler.Parse(source);

				var outfile = System.IO.File.OpenWrite(outname);
				outfile.Write(output, 0, output.Length);



			} catch(Exception ex){
				Console.WriteLine(ex.Message);
			}
		}
	}

	public class Chip8Assembler{
		Dictionary<string, int> labels = new Dictionary<string,int>();

		public byte[] Parse(string source){
			var output = new byte[0xdff];
			int pos = 0;
			int op = 0x0;
			int register;
			int linenumber = 0;
			var stringreader = new System.IO.StringReader(source);
			var line="start";
			try {
			while(!string.IsNullOrEmpty(line)){
			
				op = 0x0;
				line = stringreader.ReadLine();
				linenumber++;

				if(string.IsNullOrEmpty(line))
					break;
				
				var args = ParseLine(line);
				if (args[0].IndexOf(':') > 0)
				{
					string label = args[0];
					label = label.Remove(label.IndexOf(':'));
					labels.Add(label, pos);
				}
				
				switch(args[0]){
				case "CLS": 
					op = 0x00e0;
					break;
				case "RET":
					op = 0xee;
					break;
				case "JP":
					if(args[1] == "V0")
						op = 0xb000 | GetAddress(args[2]);
					
					op = 0x1000 | GetAddress(args[1]);
					break;
				case "CALL":
					op = 0x2000 | 0xfff & GetAddress(args[1]);
					break;
				case "SE":
					op = 0x3000 | GetRegister(args[1]) << 8 | 0xff & GetValue(args[2]);
					break;
				case "SNE":
					if (args[2][0] == 'V'){
						op = 0x9000 | GetRegister(args[1]) << 8 | GetRegister(args[2]) << 4;
						break;
					}
					if (args[1][0] == 'V'){
						register = GetRegister(args[1]);
						op = 0x4000 | register << 8 | 0xff & GetValue(args[2]);
						break;
					}
					break;
				case "LD":
					if(args[1][0] == 'V' && args[2][0] == '#') {
						op = 0x6000 | GetRegister(args[1]) << 8 | GetValue(args[2]);
						break;
					}
					if(args[1][0] == 'V' && args[2][0] == 'V') {
						op = 0x8000 | GetRegister(args[1]) << 8 | GetRegister(args[2]) << 4;
						break;
					}
					if(args[1] == "I" ) {
						op = 0xA000 | GetAddress(args[2]) << 8;
						break;
					}
					if (args[2] == "DT"){
						op = 0xf007 | GetRegister(args[1]) << 8;
						break;
					}
					if (args[2] == "K"){
						op = 0xf00a | GetRegister(args[1]) << 8;
						break;
					}
					if (args[1] == "DT"){
						op = 0xf015 | GetRegister(args[2]) << 8;
						break;
					}
					if (args[1] == "ST"){
						op = 0xf018 | GetRegister(args[2]);
						break;
					}
					if (args[1] == "F"){
						op = 0xf029 | GetRegister(args[2]) << 8;
						break;
					}
					if (args[1] == "B"){
						op = 0xf033 | GetRegister(args[2]) << 8;
						break;
					}
					if (args[1] == "[I]"){
						op = 0xf055 | GetRegister(args[2]) << 8;
						break;
					}
					if (args[2] == "[I]"){
						op = 0xf065 | GetRegister(args[1]) << 8;
						break;
					}
					break;
				case "ADD":
					if(args[1][0] == 'V' && args[2][0] == '#') {
						op = 0x7000 | GetRegister(args[1]) << 8 | GetValue(args[2]);
						break;
					}
					if(args[1][0] == 'V' && args[2][0] == 'V') {
						op = 0x8004 | GetRegister(args[1]) << 8 | GetRegister(args[2]) << 4;
						break;
					}
					break;
				case "OR":
					op = 0x8001 | GetRegister(args[1]) << 8 | GetRegister(args[2]) << 4;
					break;
				case "AND":
					op = 0x8002 | GetRegister(args[1]) << 8 | GetRegister(args[2]) << 4;
					break;
				case "XOR":
					op = 0x8003 | GetRegister(args[1]) << 8 | GetRegister(args[2]) << 4;
					break;
				case "SUB":
					op = 0x8005 | GetRegister(args[1]) << 8 | GetRegister(args[2]) << 4;
					break;
				case "SHR":
					op = 0x8006 | GetRegister(args[1]) << 8 | GetRegister(args[2]) << 4;
					break;
				case "SUBN":
					op = 0x8007 | GetRegister(args[1]) << 8 | GetRegister(args[2]) << 4;
					break;
				case "SHL":
					op = 0x800e | GetRegister(args[1]) << 8 | GetRegister(args[2]) << 4;
					break;
				case "RND":
					op = 0xc000 | GetRegister(args[1]) << 8 | GetValue(args[2]);
					break;
				case "DRW":
					op = 0xd000 | GetRegister(args[1]) << 8 | GetRegister(args[2]) << 4 | 0x0f & Convert.ToInt16(args[3], 16);
					break;
				case "SKP":
					op = 0xe093 | GetRegister(args[1]) << 8;
					break;
				case "SKNP":
					op = 0xe0a1 | GetRegister(args[1]) << 8;
					break;


				default:
					break;
				}

				if(op != 0x0) {
					output[pos++] = (byte)(op >> 8);
					output[pos++] = (byte)(op & 0xff);
				}
			}
			return output.Take(pos).ToArray();
			} catch (Exception ex){
				throw new Exception("Exception on line " + linenumber + " " + ex.Message);  
			}
		
		}

		string[] ParseLine(string line){
			line = line.Trim();
			// Empty line
			if (string.IsNullOrEmpty(line)) return new string[0];

			// Comments
			int comment = line.IndexOf(";");
			if(comment == 0)
				return new string[0];
			
			if (comment > 0){
				line = line.Substring(0, comment - 1);
			}

			var elements = line.Split(new []{ ' ', ',' });
			elements = elements.Where(e => !string.IsNullOrEmpty(e.Trim())).Select(e => e.Trim()).ToArray();

			return elements;
		}

		public int GetAddress(string arg){
			if(labels.ContainsKey(arg))
				return labels[arg];
							
			if(arg[0] != '#')
				throw new Exception("Argument is not an address. Missing #");
			return Convert.ToInt16(arg.Replace("#", ""), 16);
		}

		public int GetRegister(string arg){
			if(arg[0] != 'V')
				throw new Exception("Argument is not a register. Missing V");
			return Convert.ToInt16(arg.Replace("V", ""), 16);
		}
		public int GetValue(string arg){
			if(arg[0] != '#')
				throw new Exception("Argument is not a value. Missing #");
			return Convert.ToInt16(arg.Replace("#", ""), 16);
		}
	}
}
