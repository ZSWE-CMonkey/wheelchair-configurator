using ConfigurationLogic;

namespace WheelchairConfigurator
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();

            ITemporaryLogic temp = TemporaryFactory.CreateTemporaryLogic();

            Label tempLabel = new Label() { Text = temp.GetTemporary(), TextColor=Colors.DarkRed };
            Testing.Children.Add(tempLabel);
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
