using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace RedditBot
{
    public class Revolution
    {
        private readonly IWebDriver driver;

        public Revolution()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--disable-notifications");

            this.driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl("https://www.reddit.com/domain/old.reddit.com/");

            Start();
        }
        
        private IWebElement WaitForElement(string locator, int maxSeconds)
        {
            return new WebDriverWait(this.driver, TimeSpan.FromSeconds(maxSeconds))
                .Until(driver => driver.FindElement(By.XPath(locator)));
        }

        private IWebElement waitForElementById(string locator, int maxSeconds)
        {
            return new WebDriverWait(this.driver, TimeSpan.FromSeconds(maxSeconds))
                .Until(driver => driver.FindElement(By.Id(locator)));
        }
        
        private IWebElement waitForElementByClassSelector(string locator, int maxSeconds)
        {
            return new WebDriverWait(this.driver, TimeSpan.FromSeconds(maxSeconds))
                .Until(driver => driver.FindElement(By.CssSelector(locator)));
        }
        
        private ReadOnlyCollection<IWebElement> waitForElementsByClassSelector(string locator, int maxSeconds)
        {
            return new WebDriverWait(this.driver, TimeSpan.FromSeconds(maxSeconds))
                .Until(driver => driver.FindElements(By.CssSelector(locator)));
        }

        private ReadOnlyCollection<IWebElement> waitForElements(string locator, int maxSeconds)
        {
            return new WebDriverWait(this.driver, TimeSpan.FromSeconds(maxSeconds))
                .Until(driver => driver.FindElements(By.XPath(locator)));
        }

        private IWebElement findNestedElement(string locator, IWebElement parent, int maxSeconds)
        {
            try
            {
                return new WebDriverWait(this.driver, TimeSpan.FromSeconds(maxSeconds))
                    .Until(parent => {
                        try
                        {
                            return parent.FindElement(By.CssSelector(locator));
                        }
                        catch(Exception e)
                        {
                            return null;
                        }
                    });
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        public string Username
        {
            get
            {
                return File.ReadAllLines("Data\\Credentials.txt")[0];
            }
        }
        public string Password
        {
            get
            {
                return File.ReadAllLines("Data\\Credentials.txt")[1];
            }
        }

        private void Start()
        {
            IWebElement element = WaitForElement("//*[@id=\"header-bottom-right\"]/span[1]/a[1]", 20);
            element.Click();
            Thread.Sleep(2000);
            
            IWebElement username = WaitForElement("//*[@id=\"user_login\"]", 5);
            username.SendKeys(Username);
            Thread.Sleep(500);

            IWebElement password = WaitForElement("//*[@id=\"passwd_login\"]", 5);
            password.SendKeys(Password);
            Thread.Sleep(500);
            
            IWebElement login = WaitForElement("//*[@id=\"login-form\"]/div[5]/button", 5);
            login.Click();
            
            Thread.Sleep(500);
            NavigateToModernReddit();
            
            StartDownVoting();
        }

        private void NavigateToModernReddit()
        {
            driver.Navigate().GoToUrl("https://old.reddit.com/r/Animemes/new/");
        }

        private void Refresh()
        {
            driver.Navigate().Refresh();
        }

        private void StartDownVoting()
        {

            while (true)
            {
                Random r = new Random();
                
                Refresh();

                ReadOnlyCollection<IWebElement> posts = waitForElementsByClassSelector(".link", 20);
                Console.WriteLine(posts.Count);
                
                foreach (var post in posts)
                {
                    IWebElement downvoteButton = findNestedElement(".down", post, 1);

                    if (downvoteButton != null)
                    {
                        try
                        {
                            Console.WriteLine("Downvoting post...");
                            downvoteButton.Click();
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Not Clickable");
                        }
                    }
                    else
                    {
                        Console.WriteLine("element was null");
                    }
                    
                    Thread.Sleep(r.Next(100, 1000));
                }

                Console.WriteLine("Going to sleep, see ya in 10 seconds");
                Thread.Sleep(10000);
            }
        }
    }
}