using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WordamentSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class CellDetails
        {
            public Button button;
            public char letter;
            public bool isLocked;
            public int rowIndex, columnIndex;
            public List<string> storedResults;

            /** Purpose : Constructor to map the buttons from XAML to C#
             * 
             */ 
            public CellDetails(Button button, int rowIndex, int columnIndex)
            {
                this.button = button;
                this.rowIndex = rowIndex;
                this.columnIndex = columnIndex;
                isLocked = false;
                storedResults = new List<string>();
            }

            /** Purpose : To get the data for the content and set certain defaults for the buttons
             * 
             */ 
            public void UpdateData(char content)
            {
                this.letter = Convert.ToChar(content.ToString().ToLower());
                button.Content = content.ToString().ToUpper();
                SetButtonProperty();
                storedResults.Clear();
            }

            /** Purpose : To set the default characteristics of the buttons
             * 
             */ 
            private void SetButtonProperty()
            {
                button.IsEnabled = false;
                button.FontWeight = FontWeights.ExtraBold;
                button.Height = button.Width = 0;
                button.BorderBrush = Brushes.Coral;
                button.ToolTip = "Click on this, to display the list that starts with this box";
            }
        }

        CellDetails[,] cellObject = new CellDetails[4, 4];
        Button selectedButton;
        Stack<CellDetails> cellBuffer = new Stack<CellDetails>();
        DispatcherTimer goAnimation, selectionAnimation;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        /** Purpose : Initialization of the global variable
         * 
         */
        private void Initialize()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    cellObject[i, j] = new CellDetails(FindName("Button" + i + j) as Button, i, j);
                }
            }

            goAnimation = new DispatcherTimer(DispatcherPriority.Send);
            goAnimation.Interval = TimeSpan.FromMilliseconds(30);
            goAnimation.Tick += GoAnimation_Tick;

            selectionAnimation = new DispatcherTimer(DispatcherPriority.Send);
            selectionAnimation.Interval = TimeSpan.FromMilliseconds(30);
            selectionAnimation.Tick += SelectionAnimation_Tick;
        }

        /** Purpose : Highlight how the word is formed in the sequence
         * 
         * This is will increment the size of the buttons 
         * When the specified height is reached we set the default values and pop out the button
         */ 
        private void SelectionAnimation_Tick(object sender, EventArgs e)
        {
            if(cellBuffer.Count > 0)
            {
                Button buttonAnimated = cellBuffer.Peek().button;
                if(buttonAnimated.Height < 55)
                {
                    buttonAnimated.Height = buttonAnimated.Width += 5;
                }
                else
                {
                    buttonAnimated.Height = buttonAnimated.Width = 50;
                    buttonAnimated.BorderBrush = Brushes.BlueViolet;
                    cellBuffer.Pop();
                }
            }
            else
            {
                selectionAnimation.Stop();
            }
        }

        /** Purpose : Incrementing the size of all the buttons at once
         * 
         * After animation is completed enable all the buttons
         * The purpose of disabling the buttons until the completion of the animation is to prevent any clicks leading to garbage fields
         */ 
        private void GoAnimation_Tick(object sender, EventArgs e)
        {
            if (cellObject[0, 0].button.Height < 55)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        cellObject[i,j].button.Height = cellObject[i,j].button.Width += 5;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        cellObject[i, j].button.Height = cellObject[i, j].button.Width = 50;
                        cellObject[i, j].button.IsEnabled = true;
                    }
                }

                goAnimation.Stop();
            }
        }

        /** Purpose : List the words that start with the selected letter
         * 
         * isLocked the WordList and clear its contents
         * 
         * Assgin the sender as the Selected Button
         * Get the index of the button from it
         * Recolor all the buttons and set the selectedButon to purple
         * 
         * Check whether we have already stored the results in the buffer or not
         *      * If so then take the buffer and use it
         * Otherwise
         *      * Open the file with the name given by the content of the button
         *      * Read each word and check whether the word is a part of the sequence or not
         *      * If so, Add the word to the list
         *      * Close the file
         * 
         * Unlock the WordList
         */
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WordList.IsEnabled = false;
            WordList.Items.Clear();

            string currentWord;

            selectedButton = (sender as Button);
            ResetButtons();
            selectedButton.BorderBrush = Brushes.BlueViolet;
            int rowIndex = Convert.ToInt32(selectedButton.Name[6].ToString());
            int columnIndex = Convert.ToInt32(selectedButton.Name[7].ToString());

            if (cellObject[rowIndex, columnIndex].storedResults.Count != 0)
            {
                foreach(var result in cellObject[rowIndex, columnIndex].storedResults)
                {
                    WordList.Items.Add(result);
                }
            }
            else
            {
                FileStream file = new FileStream(@"Resources\" + (sender as Button).Content + ".kappspot", FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(file);
                while ((currentWord = reader.ReadLine()) != null)
                {
                    if (IsWordPresent(currentWord, rowIndex, columnIndex, false) == true)
                    {
                        WordList.Items.Add(currentWord);
                    }
                }
                reader.Close();
                file.Close();

                foreach (var result in WordList.Items)
                {
                    cellObject[rowIndex, columnIndex].storedResults.Add(result.ToString());
                }
            }

            WordList.IsEnabled = true;
        }

        /** Purpose : Finds and optionally stores the sequence of characters
         * 
         * Clear the buffer
         * 
         * Push the first Box into the stack and set it as used
         * 
         * Starting from the next element till
         *      * We find the word - marked by i reaching the end
         *      * We don't find the word - marked by the stack becoming empty
         *      
         * Check whether the next element is in the sequence
         *      * Goto the next character of the word
         *      * Use the element at the top
         * Otherwise
         *      * The letter used for finding the next letter will not be correct - so check other possibilities
         *      * Pop out the top of the stack and set it as not used
         *      * Now, pop out all the elements until the previously popped element and un-use it
         *      * Decrement the letter index so as to search the previous element only when they are not equal
         *
         *      
         * Check whether the no of blocks in the stack is greater than 0
         *      * If so then the word we are searching for is found
         *      * If this function is called by the user generated selection then
         *          * For the animation purpose we must store the original order of the sequence
         *          * So, every element that is in the stack and which is locked is a part of the sequence
         *          * Moreover when popped out they will be in the reverse order
         *          * So, store it in another stack - reverse(reverse) = forward
         *          * Unlock all the Boxes so that a new word can be found
         *          * Save this local stack to the original stack as we can access it anywhere - global variable
         *      * Otherwise
         *          * This was called by the computer for only listing purpose
         *          * So there is no need to save the order
         *          * Hence simply empty the stack and unlock all the items
         *      * return true
         * otherwise
         *      * The word we are searching for is not found
         *      * return false
         */ 
        private bool IsWordPresent(string currentWord, int rowIndex, int columnIndex, bool userDriven)
        {
            cellBuffer.Clear();

            cellBuffer.Push(cellObject[rowIndex, columnIndex]);
            cellBuffer.Peek().isLocked = true;

            for (int i = 1; i < currentWord.Length && cellBuffer.Count > 0; )
            {
                if (IsNextLetterAvilable(currentWord[i]))
                {
                    i++;
                    cellBuffer.Peek().isLocked = true;
                }
                else
                {
                    CellDetails cellBufferTop = cellBuffer.Pop();
                    cellBufferTop.isLocked = false;

                    while (cellBuffer.Count > 0 && cellBufferTop.letter != cellBuffer.Peek().letter)
                    {
                        cellBufferTop = cellBuffer.Pop();
                        cellBufferTop.isLocked = false;
                        i--;
                    }
                    
                    if (cellBuffer.Count > 0)
                    {
                        cellBuffer.Peek().isLocked = true;
                    }
                }
            }

            if (cellBuffer.Count > 0)
            {
                if (userDriven)
                {
                    Stack<CellDetails> userSelectedBuffer = new Stack<CellDetails>();
                    while (cellBuffer.Count > 0)
                    {
                        if (cellBuffer.Peek().isLocked == true)
                        {
                            userSelectedBuffer.Push(cellBuffer.Pop());
                            userSelectedBuffer.Peek().isLocked = false;
                            userSelectedBuffer.Peek().button.Height = userSelectedBuffer.Peek().button.Width = 0;
                        }
                        else
                        {
                            cellBuffer.Pop();
                        }
                    }

                    cellBuffer = userSelectedBuffer;
                }
                else
                {
                    while (cellBuffer.Count > 0)
                    {
                        cellBuffer.Pop().isLocked = false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /** Purpose : Selects the next possible letters and pushes them into the stack
         * 
         *  First take the previous letter and assume that the letter is not found
         *  Now, search all possible locations and add each of these which can be in the stacktree to the stack
         *  If we have found atleast one match return that a letter has been found otherwise return false
         *  Note : 
         *      * It is important to prevent the ArrayIndexOutOfBounds exception
         *      * Prevent locked items from being pushed
         */
        private bool IsNextLetterAvilable(char nextLetter)
        {
            CellDetails previousLetter = cellBuffer.Peek();
            bool letterFound = false;

            for (int i = previousLetter.rowIndex - 1; i <= previousLetter.rowIndex + 1; i++)
            {
                for (int j = previousLetter.columnIndex - 1; j <= previousLetter.columnIndex + 1; j++)
                {
                    if (i < 4 && j < 4 && i > -1 && j > -1)
                    {
                        if (cellObject[i, j].isLocked == false && nextLetter == cellObject[i, j].letter)
                        {
                            cellBuffer.Push(cellObject[i, j]);
                            letterFound = true;
                        }
                    }
                }
            }

            return letterFound;
        }

        /** Purpose : Assigning new Data and starting the GoButton Animation
         * 
         * We are now supposed to update the contents of the Buttons
         * By taking the data from the LetterArraySource
         * Reset the formatting specifications of the TextBox
         * Clear the contents of the word list in transaction lock mode
         * Start the animation of the button
         */
        private void Go_Click(object sender, RoutedEventArgs e)
        {
            for(int i=0; i<4;i++)
            {
                for(int j=0;j<4;j++)
                {
                    cellObject[i, j].UpdateData(LetterArraySource.Text[i * 4 + j]);
                }
            }

            LetterArraySource.Text = "Type the sequence here";
            LetterArraySource.FontStyle = FontStyles.Italic;
            LetterArraySource.SelectAll();

            WordList.IsEnabled = false;
            WordList.Items.Clear();
            WordList.IsEnabled = true;

            Go.Content = 0;
            Go.IsEnabled = false;

            goAnimation.Start();
        }

        /** Purpose : Maintaing the homogenity of the text and toggling the activation of Go button
         * 
         * Correct the text by eliminating all the invalid characters
         * Change the contents of the source with the next text
         * Update the contents of the Go buttton and enable or disable it
         */
        private void LetterArraySource_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LetterArraySource.Text != "Type the sequence here")
            {
                string correctedText = "";

                foreach (var letter in LetterArraySource.Text)
                {
                    if (letter >= 'a' && letter <= 'z' || letter >= 'A' && letter <= 'Z')
                    {
                        correctedText += letter;
                    }
                }

                LetterArraySource.Text = correctedText;
                LetterArraySource.Select(correctedText.Length, 0);

                if(correctedText.Length == 16)
                {
                    Go.IsEnabled = true;
                    Go.Content = "Go!";
                }
                else
                {
                    Go.IsEnabled = false;
                    Go.Content = correctedText.Length;
                }
            }
        }

        /** Purpose : Clears the contents of the Source textbox and clears its contents
         * 
         * If the text is the default text then
         *  Clear the text and set the font style to NORMAL
         */
        private void LetterArraySource_GotFocus(object sender, RoutedEventArgs e)
        {
            if (LetterArraySource.Text == "Type the sequence here")
            {
                LetterArraySource.FontStyle = FontStyles.Normal;
                LetterArraySource.Clear();
            }
        }

        /** Purpose : Pseudo Keyclick event creation for Go button via presing ENTER key
         * 
         * Enter key is used as a substitue for the Go Button
         * Indirect event activation is used when the Go button is enabled
         */ 
        private void LetterArraySource_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Go.IsEnabled == true)
                {
                    Go_Click(Go, e);
                }
            }
        }

        /** Purpose : Set the default text and style when the text box has lost its focus
         * 
         * Set the default text and stlye only when it is empty
         */
        private void LetterArraySource_LostFocus(object sender, RoutedEventArgs e)
        {
            if (LetterArraySource.Text.Length == 0)
            {
                LetterArraySource.Text = "Type the sequence here";
                LetterArraySource.FontStyle = FontStyles.Italic;
            }
        }

        /** Purpose : Animate and highlight the sequence selected
         * 
         *  Check whether the WordList is not locked
         *      * Reset the initial settings of the buttons to remove garbage settings of incomplete animation
         *      * Get the Row and Column index of the selected button
         *      * Find and save the slots in order by calling IsWordPresent() function
         *      * We have collected all the data, so start the animation 
         *          * data required is stored as the letterBuffer provided by IsWordPresent() function
         */
        private void WordList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WordList.IsEnabled == true)
            {
                ResetButtons();

                int rowIndex = Convert.ToInt32(selectedButton.Name[6].ToString());
                int columnIndex = Convert.ToInt32(selectedButton.Name[7].ToString());

                IsWordPresent((sender as ListBox).SelectedItem.ToString(), rowIndex, columnIndex, true);

                selectionAnimation.Start();
            }
        }

        /** Purpose : Reset the Height and the border brush property of the buttons
         * 
         * Note : 
         *  * Before animation the height and width must be set to 50
         *  * Because some elements which may have been involved with previous animation and not current animation
         *  * They may have insufficient height and width due to the start of another animation before the completion of predecessor
         *  * We must always reset the colors because of the same reason - the color effects of animation are hardcore
         */
        private void ResetButtons()
        { 
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    cellObject[i, j].button.BorderBrush = Brushes.Coral;
                    cellObject[i, j].button.Height = 50;
                    cellObject[i, j].button.Width = 50;
                }
            }
        }

        /** Purpose : Search in Google for the selected word's meaning
         * 
         * This will lookup for the word which is currently selected in google
         * This uses the custom google search expression as a command line argument for the web browser
         */
        private void WordList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(WordList.SelectedItem != null)
            {
                Process.Start("https://www.google.com/search?q=" + WordList.SelectedItem.ToString() + "+meaning&expnd=1");
            }
        }

        /** Purpose : Change the size of the text in the word list
         * 
         * Changes the Size of the text in the WordList
         * We must divide the value by 10 inorder to get a whole number
         */
        private void TextSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            WordList.FontSize = TextSize.Value / 10;
        }
    }
}
