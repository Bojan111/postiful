using postiful.Enums;
using postiful.Models.PinterestModels;
using postiful.Core.Entities.PinterestEntitiy;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace EnginaCode.Services.PinterestServices
{
    public class PinterestService : IPinterestService 
	{
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _applicationDbContext;

        public PinterestService(IHostEnvironment hostingEnvironment, ApplicationDbContext applicationDbContext)
        {
            _hostingEnvironment = hostingEnvironment;
            _applicationDbContext = applicationDbContext;
        }

        public IWebDriver CreatePin(CreatePinterestPin pinModel)
        {
            string imageFilePath = UploadImage(pinModel);
            string username = pinModel.Email;
            string password = pinModel.Password;

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--headless");
           

            using (IWebDriver driver = new ChromeDriver())
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                try
                {
                    driver.Navigate().GoToUrl("https://www.pinterest.com/login/");
                    driver.Manage().Window.Maximize();

                    Thread.Sleep(2000);

                    IWebElement usernameField = driver.FindElement(By.Name("id"));
                    usernameField.SendKeys(username);

                    IWebElement passwordField = driver.FindElement(By.Name("password"));
                    passwordField.SendKeys(password);

                    // Submit the login form
                    passwordField.SendKeys(Keys.Return);

                    Thread.Sleep(3000);

                    PinCreationEnum selectedPinType = pinModel.SelectedCreationPin;
                    switch (selectedPinType)
                    {
                        case PinCreationEnum.CreatePinForAd:
                            CreatePinForAd(driver, wait, pinModel, imageFilePath);
                            //CreatePinsFromDesktop(driver, wait, pinModel, 2);
                            break;
                        case PinCreationEnum.CreatePinForIdea:
                            // working...
                            break;
                        case PinCreationEnum.CreatePinOrganic:
                            //CreatePinForOrganic(driver, wait, pinModel, imageFilePath);
                            CreatePinsFromDesktop(driver, wait, pinModel, 2);
                            break;
                        default:
                            break;
                    }
                }
                finally
                {
                    driver.Navigate().GoToUrl($"https://www.pinterest.com/{pinModel.Username}/");

                    Thread.Sleep(3000);
                    driver.Quit();
                }
                return driver;
            }
        }

       
        // Private methods
        private string UploadImage(CreatePinterestPin model)
        {
            if (model.ImageFile != null)
            {
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ImageFile.FileName)}";

                // Extract the original filename before the first underscore
                var originalFileName = fileName.Substring(fileName.IndexOf('_') + 1);

                // Set the path to save the file
                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "images", originalFileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImageFile.CopyTo(stream);
                }
                return filePath;
            }
            return "";
        }

        private IWebDriver CreatePinForAd(IWebDriver driver, WebDriverWait wait, CreatePinterestPin pinModel, string imagePath)
        {
            Pinterest pinterestEntity = new();
            pinterestEntity.Username = pinModel.Username;
            pinterestEntity.Email = pinModel.Email;
            pinterestEntity.Password = pinModel.Password;
            pinterestEntity.Title = pinModel.Title;
            pinterestEntity.Description = pinModel.Description;
            if (pinModel.ImageFile != null && pinModel.ImageFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    pinModel.ImageFile.CopyTo(memoryStream);
                    pinterestEntity.ImageFile = memoryStream.ToArray();
                }
            }
            else
            {
                pinterestEntity.ImageFile = null;
            }

            pinterestEntity.DestinationLink = pinModel.DestinationLink;


            driver.Navigate().GoToUrl("https://www.pinterest.com/pin-builder/");
            Thread.Sleep(3000);

            // Create Title
            IWebElement titleField = wait.Until(d => d.FindElement(By.XPath("//textarea[contains(@class, 'TextArea__textArea')]")));
            titleField.SendKeys(pinModel.Title);
            Thread.Sleep(1000);

            // Create Description
            IWebElement descriptionField = wait.Until(d => d.FindElement(By.XPath("//div[contains(@aria-label, 'Tell everyone what your Pin is about')]")));
            descriptionField.SendKeys(pinModel.Description);
            Thread.Sleep(1000);

            // Create Destination Link
            IWebElement destinationLinkField = wait.Until(d => d.FindElement(By.XPath("//textarea[contains(@class, 'TextArea__medium')]")));
            destinationLinkField.SendKeys(pinModel.DestinationLink);
            Thread.Sleep(1000);

            // Upload Image (Handling file upload can be tricky with Selenium)
            IWebElement fileUploadField = wait.Until(d => d.FindElement(By.XPath("//input[contains(@aria-label, 'File upload')]")));
            fileUploadField.SendKeys(imagePath);
            Thread.Sleep(1000);


            // Click on the board dropdown
            IWebElement dropdown = driver.FindElement(By.XPath("//button[@data-test-id='board-dropdown-select-button']"));
            dropdown.Click();
            Thread.Sleep(2000);

            IReadOnlyCollection<IWebElement> dropdownElements = wait.Until(d => d.FindElements(By.XPath("//div[@data-test-id='boardWithoutSection' or @data-test-id='boardWithSection']")));
            if (dropdownElements != null)
            {
                List<string> boardNames = new List<string>();


                foreach (IWebElement dropdownElement in dropdownElements)
                {
                    string boardName = dropdownElement.Text.Trim();
                    //pinModel.BoardsList.Add(new SelectListItem { Text = boardName, Value = boardName });
                }

                IWebElement secondDropdownElement = dropdownElements.ToArray()[1];
                IWebElement buttonElement = secondDropdownElement.FindElement(By.XPath(".//div[@role='button']"));
                buttonElement.Click();
                Thread.Sleep(2000);
            }


            // Create Pin (publish)
            IWebElement publishButton = driver.FindElement(By.XPath("//button[@data-test-id='board-dropdown-save-button']"));
            publishButton.Click();

            Thread.Sleep(5000);

            driver.Navigate().GoToUrl($"https://www.pinterest.com/{pinModel.Username}/");
            Thread.Sleep(3000);

            _applicationDbContext.Pinterests.Add(pinterestEntity);
            _applicationDbContext.SaveChanges();
            return driver;
        }

        private IWebDriver CreatePinForOrganic(IWebDriver driver, WebDriverWait wait, CreatePinterestPin pinModel, string imagePath)
        {
            driver.Navigate().GoToUrl("https://www.pinterest.com/pin-creation-tool/");
            Thread.Sleep(8000);

            // Upload Image (Handling file upload can be tricky with Selenium)
            IWebElement fileUploadField = wait.Until(d => d.FindElement(By.XPath("//input[@data-test-id='storyboard-upload-input']")));
            fileUploadField.SendKeys(imagePath);
            Thread.Sleep(2000);

            // Create Title
            IWebElement titleField = wait.Until(d => d.FindElement(By.XPath("//input[@id='storyboard-selector-title']")));
            titleField.SendKeys(pinModel.Title);
            Thread.Sleep(1000);

            // Create Description
            IWebElement descriptionField = wait.Until(d => d.FindElement(By.XPath("//div[contains(@aria-label, 'Add a detailed description')]")));
            descriptionField.SendKeys(pinModel.Description);
            Thread.Sleep(1000);

            // Create Destination Link
            IWebElement destinationLinkField = wait.Until(d => d.FindElement(By.XPath("//input[@id='WebsiteField']")));
            destinationLinkField.SendKeys(pinModel.DestinationLink);
            Thread.Sleep(1000);


            // Click on the board dropdown
            IWebElement dropdown = driver.FindElement(By.XPath("//button[@data-test-id='board-dropdown-select-button']"));
            dropdown.Click();
            Thread.Sleep(2000);

            IReadOnlyCollection<IWebElement> dropdownElements = wait.Until(d => d.FindElements(By.XPath("//div[@data-test-id='boardWithoutSection' or @data-test-id='boardWithSection']")));
            List<string> boardNames = new List<string>();


            foreach (IWebElement dropdownElement in dropdownElements)
            {
                string boardName = dropdownElement.Text.Trim();
                //pinModel.BoardsList.Add(new SelectListItem { Text = boardName, Value = boardName });
            }

            IWebElement secondDropdownElement = dropdownElements.ToArray()[1];

            // Select the first board from the dropdown
            IWebElement buttonElement = secondDropdownElement.FindElement(By.XPath(".//div[@role='button']"));
            buttonElement.Click();
            Thread.Sleep(2000);

            // Create Pin (publish)
            IWebElement publishButton = driver.FindElement(By.XPath("//div[@data-test-id='storyboard-creation-nav-done']"));
            publishButton.Click();

            Thread.Sleep(5000);

            // Navigate to your Pinterest profile (Update this URL with the correct URL for your profile)
            driver.Navigate().GoToUrl($"https://www.pinterest.com/{pinModel.Username}/");
            Thread.Sleep(3000);
            return driver;
        }

        private IWebDriver CreatePinsFromDesktop(IWebDriver driver, WebDriverWait wait, CreatePinterestPin pinModel, int numberOfPinsToCreate)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string folder = Path.Combine(desktopPath, "Automation");

            if (Directory.Exists(folder))
            {
                string[] subfolders = Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly);

                foreach (string subfolder in subfolders)
                {
                    string[] imageFiles = Directory.GetFiles(subfolder, "*.jpg");
                    string[] textFiles = Directory.GetFiles(subfolder, "*.txt");

                    if (textFiles.Length > 0)
                    {
                        foreach (string textFilePath in textFiles)
                        {
                            string[] lines = File.ReadAllLines(textFilePath);

                            if (lines.Length >= 3)
                            {
                                pinModel.Title = lines[1];
                                pinModel.Description = lines[4];
                                pinModel.DestinationLink = lines[7];


                                if (imageFiles.Length > 0)
                                {
                                    string imageFilePath = imageFiles[0];

                                    //CreatePinForOrganicTest(driver, wait, pinModel, imageFilePath, numberOfPinsToCreate);
                                    CreatePinForAdTest(driver, wait, pinModel, imageFilePath, numberOfPinsToCreate);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Insufficient lines in text file: {textFilePath}");
                            }
                        }
                    }
                }
            }
            return driver;
        }

        private IWebDriver CreatePinForAdTest(IWebDriver driver, WebDriverWait wait, CreatePinterestPin pinModel, string imagePath, int numberOfPinsToCreate)
        {
            for (int i = 0; i < numberOfPinsToCreate; i++)
            {
                driver.Navigate().GoToUrl("https://www.pinterest.com/pin-builder/");
                Thread.Sleep(3000);

                // Create Title
                IWebElement titleField = wait.Until(d => d.FindElement(By.XPath("//textarea[contains(@class, 'TextArea__textArea')]")));
                titleField.SendKeys(pinModel.Title);
                Thread.Sleep(1000);

                // Create Description
                IWebElement descriptionField = wait.Until(d => d.FindElement(By.XPath("//div[contains(@aria-label, 'Tell everyone what your Pin is about')]")));
                descriptionField.SendKeys(pinModel.Description);
                Thread.Sleep(1000);

                // Create Destination Link
                IWebElement destinationLinkField = wait.Until(d => d.FindElement(By.XPath("//textarea[contains(@class, 'TextArea__medium')]")));
                destinationLinkField.SendKeys(pinModel.DestinationLink);
                Thread.Sleep(1000);

                // Upload Image (Handling file upload can be tricky with Selenium)
                IWebElement fileUploadField = wait.Until(d => d.FindElement(By.XPath("//input[contains(@aria-label, 'File upload')]")));
                fileUploadField.SendKeys(imagePath);
                Thread.Sleep(1000);


                // Click on the board dropdown
                IWebElement dropdown = driver.FindElement(By.XPath("//button[@data-test-id='board-dropdown-select-button']"));
                dropdown.Click();
                Thread.Sleep(2000);

                IReadOnlyCollection<IWebElement> dropdownElements = wait.Until(d => d.FindElements(By.XPath("//div[@data-test-id='boardWithoutSection' or @data-test-id='boardWithSection']")));
                if (dropdownElements != null)
                {
                    List<string> boardNames = new List<string>();


                    foreach (IWebElement dropdownElement in dropdownElements)
                    {
                        string boardName = dropdownElement.Text.Trim();
                        //pinModel.BoardsList.Add(new SelectListItem { Text = boardName, Value = boardName });
                    }

                    IWebElement secondDropdownElement = dropdownElements.ToArray()[0];
                    IWebElement buttonElement = secondDropdownElement.FindElement(By.XPath(".//div[@role='button']"));
                    buttonElement.Click();
                    Thread.Sleep(2000);
                }


                // Create Pin (publish)
                IWebElement publishButton = driver.FindElement(By.XPath("//button[@data-test-id='board-dropdown-save-button']"));
                publishButton.Click();

                Thread.Sleep(5000);

                driver.Navigate().GoToUrl($"https://www.pinterest.com/{pinModel.Username}/");
                Thread.Sleep(3000);
            }
            return driver;
        }

        private IWebDriver CreatePinForOrganicTest(IWebDriver driver, WebDriverWait wait, CreatePinterestPin pinModel, string imagePath, int numberOfPinsToCreate)
        {
            for (int i = 0; i < numberOfPinsToCreate; i++)
            {
                driver.Navigate().GoToUrl("https://www.pinterest.com/pin-creation-tool/");
                Thread.Sleep(8000);

                // Upload Image (Handling file upload can be tricky with Selenium)
                IWebElement fileUploadField = wait.Until(d => d.FindElement(By.XPath("//input[@data-test-id='storyboard-upload-input']")));
                fileUploadField.SendKeys(imagePath);
                Thread.Sleep(2000);

                // Create Title
                IWebElement titleField = wait.Until(d => d.FindElement(By.XPath("//input[@id='storyboard-selector-title']")));
                titleField.SendKeys(pinModel.Title);
                Thread.Sleep(1000);

                // Create Description
                IWebElement descriptionField = wait.Until(d => d.FindElement(By.XPath("//div[contains(@aria-label, 'Add a detailed description')]")));
                descriptionField.SendKeys(pinModel.Description);
                Thread.Sleep(1000);

                // Create Destination Link
                IWebElement destinationLinkField = wait.Until(d => d.FindElement(By.XPath("//input[@id='WebsiteField']")));
                destinationLinkField.SendKeys(pinModel.DestinationLink);
                Thread.Sleep(1000);


                //// Click on the board dropdown
                //IWebElement dropdown = driver.FindElement(By.XPath("//button[@data-test-id='board-dropdown-select-button']"));
                //dropdown.Click();
                //Thread.Sleep(2000);

                //IReadOnlyCollection<IWebElement> dropdownElements = wait.Until(d => d.FindElements(By.XPath("//div[@data-test-id='boardWithoutSection' or @data-test-id='boardWithSection']")));
                //List<string> boardNames = new List<string>();


                //foreach (IWebElement dropdownElement in dropdownElements)
                //{
                //    string boardName = dropdownElement.Text.Trim();
                //    //pinModel.BoardsList.Add(new SelectListItem { Text = boardName, Value = boardName });
                //}

                //IWebElement secondDropdownElement = dropdownElements.ToArray()[1];

                //// Select the first board from the dropdown
                //IWebElement buttonElement = secondDropdownElement.FindElement(By.XPath(".//div[@role='button']"));
                //buttonElement.Click();
                //Thread.Sleep(2000);

                // Create Pin (publish)
                IWebElement publishButton = driver.FindElement(By.XPath("//div[@data-test-id='storyboard-creation-nav-done']"));
                publishButton.Click();

                Thread.Sleep(5000);
            }
            return driver;
        }

    }
}