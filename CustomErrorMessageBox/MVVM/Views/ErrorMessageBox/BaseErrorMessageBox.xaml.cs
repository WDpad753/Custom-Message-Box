using CustomErrorMessageBox.MVVM.Models;
using CustomErrorMessageBox.MVVM.Views.ErrorMessageBox;
using CustomErrorMessageBox.Styles.UICommands.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CustomErrorMessageBox.MVVM.Views.ErrorMessageBox
{
    /// <summary>
    /// Interaction logic for BaseErrorMessageBox.xaml
    /// </summary>
    public partial class BaseErrorMessageBox : Window
    {
        private Point startPoint;
        private double initialHorizontalOffset;
        private double initialVerticalOffset;
        private bool draggingPopup = false;
        private Point initialMousePosition;
        private Point startDragPoint;
        static BaseErrorMessageBox? errorMessageBox;
        static DialogResults result = DialogResults.No;

        public BaseErrorMessageBox()
        {
            InitializeComponent();
        }

        public void Show(Exception ex)
        {
            var buttons = new List<DialogButtons>()
            {
                DialogButtons.Ok
            };

            errorMessageBox = new BaseErrorMessageBox();
            errorMessageBox.txtBlkErrorMessage.Text = ex.Message;
            errorMessageBox.txtBlkErrorCallStack.Text = ex.StackTrace;
            errorMessageBox.txtTitle.Text = errorMessageBox.GetTitle(DialogTitle.Error);

            // Clear any existing buttons (if reused)
            errorMessageBox.ButtonsPanel.Children.Clear();

            // Adding buttons based on the params list:
            foreach (var button in buttons)
            {
                string buttonText = GetButton(button); // Get the display text for the button using your method
                AddButton(buttonText, button);         // Add the button to the custom message box
            }

            errorMessageBox.iconMsg.Kind = MaterialDesignThemes.Wpf.PackIconKind.Error;
            errorMessageBox.iconMsg.Foreground = Brushes.DarkRed;

            errorMessageBox.ShowDialog(); // Use ShowDialog to show the window
        }

        private string GetTitle(DialogTitle value)
        {
            return Enum.GetName(typeof(DialogTitle), value);
        }

        private static string GetButton(DialogButtons value)
        {
            return Enum.GetName(typeof(DialogButtons), value);
        }

        private static void AddButton(string buttonText, DialogButtons dialogButton)
        {
            // Create the button
            var button = new RoundButton
            {
                Content = buttonText,
                Width = 100,
                Height = 30,
                Background = dialogButton switch
                {
                    DialogButtons.Yes => Brushes.DarkGreen,
                    DialogButtons.No => Brushes.DarkRed,
                    DialogButtons.Ok => Brushes.Blue,
                    DialogButtons.Cancel => Brushes.Gray,
                    _ => Brushes.LightGray
                },
                Foreground = Brushes.White,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FocusVisualStyle=null,
                Cursor = Cursors.Hand
                //CornerRadius=new CornerRadius(50)
            };

            // Create a Style to Remove Hover Effects
            Style noHoverStyle = new Style(typeof(RoundButton));
            noHoverStyle.Setters.Add(new Setter(RoundButton.BackgroundProperty, Brushes.LightGray));
            noHoverStyle.Setters.Add(new Setter(RoundButton.BorderBrushProperty, Brushes.Black));
            noHoverStyle.Setters.Add(new Setter(RoundButton.BorderThicknessProperty, new Thickness(1)));
            noHoverStyle.Setters.Add(new Setter(RoundButton.CornerRadiusProperty, new CornerRadius(10)));

            // Remove MouseOver Trigger
            ControlTemplate template = new ControlTemplate(typeof(RoundButton));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));

            // Borders
            border.Name = "border";
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(RoundButton.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(RoundButton.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(RoundButton.BorderThicknessProperty));
            border.SetValue(Border.CornerRadiusProperty, new TemplateBindingExtension(RoundButton.CornerRadiusProperty));
            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(contentPresenter);
            template.VisualTree = border;

            // Triggers 
            Trigger mouseOverTrue = new Trigger()
            {
                Property = UIElement.IsMouseOverProperty,
                Value = true
            };
            mouseOverTrue.Setters.Add(new Setter(UIElement.OpacityProperty, 0.5, border.Name));
            template.Triggers.Add(mouseOverTrue);

            Trigger mouseOverFalse = new Trigger()
            {
                Property = UIElement.IsMouseOverProperty,
                Value = false
            };
            mouseOverFalse.Setters.Add(new Setter(UIElement.OpacityProperty, 1.0, border.Name));
            template.Triggers.Add(mouseOverFalse);

            Trigger mousePressed = new Trigger()
            {
                Property = RoundButton.IsPressedProperty,
                Value = true
            };
            mousePressed.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(0), border.Name));
            template.Triggers.Add(mousePressed);

            // Adding template to the button
            noHoverStyle.Setters.Add(new Setter(RoundButton.TemplateProperty, template));

            // Apply Style to the Button
            button.Style = noHoverStyle;

            // Set the click event to set the result and close the dialog
            button.Click += (sender, e) =>
            {
                result = dialogButton switch
                {
                    DialogButtons.Yes => DialogResults.Yes,
                    DialogButtons.No => DialogResults.No,
                    DialogButtons.Ok => DialogResults.None,
                    DialogButtons.Cancel => DialogResults.Cancel,
                    _ => DialogResults.None
                };

                errorMessageBox?.Close();
            };

            // Add the button to the ButtonsPanel
            errorMessageBox?.ButtonsPanel.Children.Add(button);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggingPopup)
            {
                Point currentMousePosition = e.GetPosition(this); // Get the mouse position relative to the UserControl
                double offsetX = currentMousePosition.X - initialMousePosition.X;
                double offsetY = currentMousePosition.Y - initialMousePosition.Y;

                // Only move the window if the mouse has moved by a certain threshold
                if (Math.Abs(offsetX) > 1 || Math.Abs(offsetY) > 1)
                {
                    Left += offsetX;
                    Top += offsetY;

                    initialMousePosition = currentMousePosition; // Update initial position
                }
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                draggingPopup = false;
                Mouse.Capture(null);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            draggingPopup = true;
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
