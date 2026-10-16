using DopplerHunter.Views.Controls;
using System.Windows;

namespace DopplerHunter.Tests.Views.Controls
{
    public class StatCardTests
    {
        [Fact]
        public void Should_SetAndGetProperties_WhenAssigned()
        {
            RunOnSta(() =>
            {
                // Arrange
                var card = new StatCard();
                var title = "Total Files";
                var value = 42;
                var valueStyle = new Style(typeof(System.Windows.Controls.TextBlock));
                var titleStyle = new Style(typeof(System.Windows.Controls.TextBlock));

                // Act
                card.Title = title;
                card.Value = value;
                card.ValueStyle = valueStyle;
                card.TitleStyle = titleStyle;

                // Assert
                Assert.Equal(title, card.Title);
                Assert.Equal(value, card.Value);
                Assert.Same(valueStyle, card.ValueStyle);
                Assert.Same(titleStyle, card.TitleStyle);
            });
        }

        [Fact]
        public void Should_HaveDefaultValues_WhenConstructed()
        {
            RunOnSta(() =>
            {
                var card = new StatCard();

                Assert.Equal(string.Empty, card.Title);
                Assert.Null(card.Value);
                Assert.Null(card.ValueStyle);
                Assert.Null(card.TitleStyle);
            });
        }

        private static void RunOnSta(Action action)
        {
            Exception? exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }
    }
}
