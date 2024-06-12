using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Views.Modals
{
    public partial class CalculatorModal : Page
    {
        public CalculatorModal()
        {
            InitializeComponent();
            Clear_Click(null, null);
        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
            {
                txtExpression.Text += button.Content.ToString();
            }
        }

        private void Operator_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
            {
                txtExpression.Text += " " + button.Content.ToString() + " ";
            }
        }

        private void Equal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var expression = txtExpression.Text;
                var result = new DataTable().Compute(expression, null);
                txtResult.Text = result.ToString();
            }
            catch (Exception ex)
            {
                txtResult.Text = "Ошибка";
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            txtExpression.Text = string.Empty;
            txtResult.Text = string.Empty;
        }
    }
}
