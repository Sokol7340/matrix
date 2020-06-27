using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

class MatrixSolver
{
    public struct MulData
    {
        public double[] col;
        public double[] row;
    }

    public static void MulMatrix(string file1, string file2)
    {
        Task<double[]> readRow = new Task<double[]>((number) =>
        {
            double[] result;
            var input = new StreamReader(file1);
            int rows = int.Parse(input.ReadLine());
            if (rows < (int)number)
                throw new ArgumentException();

            int i = 0;
            string line = input.ReadLine();
            while (i < (int)number)
            {
                i++;
                input.ReadLine();
            }
            string[] values = line.Split(' ');
            result = new double[values.Length];
            for (int j = 0; j < values.Length; j++)
                result[j] = double.Parse(values[j]);

            return result;

        }, 0);

        Task<double[]> readCol = new Task<double[]>((number) =>
        {
            double[] result;
            var input = new StreamReader(file2);
            string line = input.ReadLine();
            int rows = int.Parse(line.Split(' ')[0]);
            int cols = int.Parse(line.Split(' ')[1]);
            if (cols < (int)number)
                throw new ArgumentException();

            result = new double[rows];
            for (int j = 0; j < rows; j++)
            {
                line = input.ReadLine();
                string[] values = line.Split(' ');
                result[j] = double.Parse(values[j]);
            }

            return result;

        }, 0);

        Task[] readTasks = new Task[] { readRow, readCol };
        foreach (var t in readTasks)
        {
            t.Start();
        }

        Task.WaitAll(readTasks);

        MulData data = new MulData()
        {
            row = readRow.Result,
            col = readCol.Result
        };


        Task<double> mul = new Task<double>((x) =>
        {
            double result = 0;
            MulData datamatrix = (MulData)x;
            for (int i = 0; i < datamatrix.row.Length; i++)
            {
                result += datamatrix.row[i] * datamatrix.col[i];
            }
            return result;
        }, data);

        Console.WriteLine(mul.Result.ToString());
    }

    static double[] ReadRow(string filename, int row)
    {
        double[] result;
        var sr = new StreamReader(filename);

        string[] input = sr.ReadLine().Split(' ');
        int rows = int.Parse(input[0]);


        string line = sr.ReadLine();
        while (row > 0)
        {
            row--;
            line = sr.ReadLine();
        }
        string[] values = line.Split(' ');
        result = new double[values.Length];
        for (int j = 0; j < values.Length; j++)
            result[j] = double.Parse(values[j]);

        return result;
    }

    static double[] ReadCol(string filename, int col)
    {
        double[] result;
        var input = new StreamReader(filename);
        string line = input.ReadLine();
        int rows = int.Parse(line.Split(' ')[0]);

        result = new double[rows];
        for (int j = 0; j < rows; j++)
        {
            line = input.ReadLine();
            string[] values = line.Split(' ');
            result[j] = double.Parse(values[col]);
        }

        return result;
    }
    //-------------

    static async Task<double> MulAsync(string inputLeft, string inputRight, int row, int col)
    {
        double[] rowData, colData;

        rowData = await Task.Run(() => ReadRow(inputLeft, row));
        colData = await Task.Run(() => ReadCol(inputRight, col));

        return await Task.Run(() => Mul(rowData, colData));
    }

    private static double Mul(double[] rowData, double[] colData)
    {
        double result = 0;
        
        for (int i = 0; i < rowData.Length; i++)
        {
            result += rowData[i] * colData[i];
        }
        return result;
    }

    public static async void Solve(string inputLeft, string inputRight, string output)
    {
        /// 1000x1000
        /// m n
        /// a_0_0 ... a_0_n-1
        /// ...
        /// a_m-1_0... a_m-1_n-1
            
        StreamReader sr = File.OpenText(inputLeft);

        string[] input = sr.ReadLine().Split(' ');
        int rowsLeft = int.Parse(input[0]);
        int colsLeft = int.Parse(input[1]);
        sr.Close();

        sr = File.OpenText(inputRight);
        input = sr.ReadLine().Split(' ');
        int rowsRight = int.Parse(input[0]);
        int colsRight = int.Parse(input[1]);
        sr.Close();

        StreamWriter sw = new StreamWriter(output);
        for (int i = 0; i < rowsLeft; i++)
        {
            Task<double>[] mulTasks = new Task<double>[colsRight];

            for (int j = 0; j < colsRight; j++)
            {
                mulTasks[j] = MulAsync(inputLeft, inputRight, i, j);
            }

            double[] results = Task.WhenAll<double>(mulTasks).Result;                
            sw.WriteLine(String.Join(" ", results));
        }
        sw.Close();
    }
}
