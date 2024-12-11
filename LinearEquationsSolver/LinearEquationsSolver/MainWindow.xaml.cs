using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Windows.Documents;

namespace LinearEquationsSolver
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); // Инициализация компонентов окна
        }

        // Модель уравнения, содержащая коэффициенты A, B, C и результат
        public class EquationModel
        {
            public double A { get; set; }
            public double B { get; set; }
            public double C { get; set; }
            public double Result { get; set; }
        }

        // Обработчик события нажатия кнопки "Решить" для ввода уравнений с клавиатуры
        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получение количества уравнений, введенных пользователем
                int equationsCount = int.Parse(EquationsCountTextBox.Text);
                // Разделение текста из TextBox на отдельные строки
                string[] equations = EquationsTextBox.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                // Проверка на соответствие количества уравнений
                if (equations.Length != equationsCount)
                {
                    MessageBox.Show("Количество введенных уравнений не соответствует числу, указанному выше.");
                    return;
                }

                // Парсинг уравнений для получения коэффициентов
                double[,] coefficients = ParseEquations(equations, equationsCount);
                // Решение системы уравнений методом итераций
                double[] results = SolveUsingIterationMethod(coefficients);
                // Вывод результатов
                DisplayResults(results, ResultsTextBlock);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Обработчик события нажатия кнопки "Загрузить файл"
        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Чтение уравнений из файла
                    string[] equations = File.ReadAllLines(openFileDialog.FileName);
                    List<EquationModel> equationModels = new List<EquationModel>();

                    // Преобразование каждой строки файла в модель уравнения
                    foreach (var line in equations)
                    {
                        var parts = line.Split(' ');
                        equationModels.Add(new EquationModel
                        {
                            A = double.Parse(parts[0]),
                            B = double.Parse(parts[1]),
                            C = double.Parse(parts[2]),
                            Result = double.Parse(parts[3])
                        });
                    }

                    // Установка данных для отображения в DataGrid
                    EquationsDataGrid.ItemsSource = equationModels;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при чтении файла: {ex.Message}");
                }
            }
        }

        // Обработчик события нажатия кнопки "Решить" для данных из GridView
        private void SolveGridButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получение уравнений из DataGrid
                var equations = new List<EquationModel>((IEnumerable<EquationModel>)EquationsDataGrid.ItemsSource);
                double[,] coefficients = new double[equations.Count, equations.Count + 1];

                // Заполнение массива коэффициентов из данных модели
                for (int i = 0; i < equations.Count; i++)
                {
                    coefficients[i, 0] = equations[i].A;
                    coefficients[i, 1] = equations[i].B;
                    coefficients[i, 2] = equations[i].C;
                    coefficients[i, 3] = equations[i].Result;
                }

                // Решение системы уравнений методом итераций
                double[] results = SolveUsingIterationMethod(coefficients);
                // Вывод результатов
                DisplayResults(results, FileResultsTextBlock);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Метод для разбора строк уравнений и получения коэффициентов
        private double[,] ParseEquations(string[] equations, int equationsCount)
        {
            double[,] coefficients = new double[equationsCount, equationsCount + 1];

            for (int i = 0; i < equationsCount; i++)
            {
                // Разделение строки уравнения на отдельные части
                string[] parts = equations[i].Trim().Split(' ');
                for (int j = 0; j < parts.Length; j++)
                {
                    coefficients[i, j] = double.Parse(parts[j]);
                }
            }

            return coefficients;
        }

        int fl = 0; // Флаг для обозначения состояния решения системы

        // Метод решения системы уравнений методом итераций
        private double[] SolveUsingIterationMethod(double[,] coefficients)
        {
            int n = coefficients.GetLength(0);
            double[] x = new double[n];
            double[] newX = new double[n];
            double epsilon = 1e-6; // Погрешность
            bool converge = false;

            // Проверка на наличие бесконечного числа решений или их отсутствие
            if (IsSystemUnderdetermined(coefficients))
            {
                MessageBox.Show("Уравнение имеет бесконечное количество решений.");
                fl = 1;
                return null; // Завершаем выполнение, если обнаружили бесконечное количество решений
            }
            if (IsSystemInconsistent(coefficients))
            {
                MessageBox.Show("Система уравнений не имеет решений.");
                fl = 2;
                return null; // Завершаем выполнение, если обнаружили, что решений нет
            }

            while (!converge)
            {
                converge = true;
                for (int i = 0; i < n; i++)
                {
                    double sum = coefficients[i, n];
                    for (int j = 0; j < n; j++)
                    {
                        if (i != j) sum -= coefficients[i, j] * x[j];
                    }

                    if (coefficients[i, i] == 0)
                    {
                        MessageBox.Show("Уравнение имеет бесконечное количество решений.");
                        return null;
                    }

                    newX[i] = sum / coefficients[i, i];

                    if (double.IsNaN(newX[i]) || double.IsInfinity(newX[i]))
                    {
                        MessageBox.Show("Уравнение имеет бесконечное количество решений.");
                        return null;
                    }

                    if (Math.Abs(newX[i] - x[i]) > epsilon)
                        converge = false;
                }
                Array.Copy(newX, x, n);
            }

            return x;
        }

        // Проверка на наличие бесконечного числа решений
        private bool IsSystemUnderdetermined(double[,] coefficients)
        {
            for (int i = 0; i < coefficients.GetLength(0); i++)
            {
                bool allZero = true;
                for (int j = 0; j < coefficients.GetLength(1) - 1; j++)
                {
                    if (coefficients[i, j] != 0)
                    {
                        allZero = false;
                        break;
                    }
                }
                if (allZero && coefficients[i, coefficients.GetLength(1) - 1] == 0)
                {
                    return true; // Возвращает true, если обнаружили строку, состоящую из всех нулей, и результат равен нулю
                }
            }
            return false;
        }

        // Проверка на наличие противоречий в системе уравнений
        private bool IsSystemInconsistent(double[,] coefficients)
        {
            for (int i = 0; i < coefficients.GetLength(0); i++)
            {
                bool allZeroCoefficients = true;
                for (int j = 0; j < coefficients.GetLength(1) - 1; j++)
                {
                    if (coefficients[i, j] != 0)
                    {
                        allZeroCoefficients = false;
                        break;
                    }
                }
                if (allZeroCoefficients && coefficients[i, coefficients.GetLength(1) - 1] != 0)
                {
                    return true; // Возвращает true, если обнаружили строку, где все коэффициенты равны нулю, но результат не равен нулю
                }
            }
            return false;
        }

        // Метод для вывода результатов на экран
        private void DisplayResults(double[] results, TextBlock textBlock)
        {
            if (results == null)
            {
                if (fl == 1)
                {
                    textBlock.Text = "Бесконечное количество решений.";
                }
                else
                {
                    textBlock.Text = "Нет решений.";
                }
                return;
            }

            textBlock.Text = "Решения:\n";
            for (int i = 0; i < results.Length; i++)
            {
                textBlock.Text += $"x{i + 1} = {results[i]:F3}\n";
            }
        }

        // Печать результатов на принтер
        private void PrintResults_Click(object sender, RoutedEventArgs e)
        {
            PrintContent(ResultsTextBlock);
        }

        private void PrintFileResults_Click(object sender, RoutedEventArgs e)
        {
            PrintContent(FileResultsTextBlock);
        }

        // Утилитарный метод для печати содержимого
        private void PrintContent(TextBlock textBlock)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                FlowDocument doc = new FlowDocument(new Paragraph(new Run(textBlock.Text)));
                doc.PagePadding = new Thickness(50);
                doc.ColumnWidth = printDialog.PrintableAreaWidth;
                doc.PageWidth = printDialog.PrintableAreaWidth;

                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "Результаты");
            }
        }

        // Сохранение результатов в файл
        private void SaveResultsToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveContentToFile(ResultsTextBlock.Text);
        }

        private void SaveFileResultsToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveContentToFile(FileResultsTextBlock.Text);
        }

        // Утилитарный метод для сохранения содержимого в файл
        private void SaveContentToFile(string content)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, content);
                    MessageBox.Show("Результаты успешно сохранены.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}");
                }
            }
        }
    }
}