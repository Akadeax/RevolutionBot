using System;
using System.Collections.ObjectModel;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace RedditBot
{
    public class Revolution
    {
        private IWebDriver driver;

        public Revolution()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--disable-notifications");

            this.driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl("https://www.reddit.com/domain/old.reddit.com/");

            Start();
        }
        
        private IWebElement waitForElement(string locator, int maxSeconds)
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

        private IWebElement findNestedElment(string locator, IWebElement parent, int maxSeconds)
        {
            try
            {
                return new WebDriverWait(this.driver, TimeSpan.FromSeconds(maxSeconds))
                    .Until(parent => parent.FindElement(By.CssSelector(locator)));
            }
            catch (Exception error)
            {
                return null;
            }
            
        }

        private void Start()
        {
            IWebElement element = waitForElement("//*[@id=\"header-bottom-right\"]/span[1]/a[1]", 20);
            element.Click();
            
            IWebElement username = waitForElement("//*[@id=\"user_login\"]", 5);
            username.SendKeys(CONFIG.USERNAME);

            IWebElement password = waitForElement("//*[@id=\"passwd_login\"]", 5);
            password.SendKeys(CONFIG.PASSWORD);
            
            IWebElement login = waitForElement("//*[@id=\"login-form\"]/div[5]/button", 5);
            login.Click();
            
            Thread.Sleep(5000);
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
            string lastPostId = "";

            while (true)
            {
                Random r = new Random();
                
                Refresh();

                ReadOnlyCollection<IWebElement> posts = waitForElementsByClassSelector(".link", 20);
                Console.WriteLine(posts.Count);
                
                foreach (var post in posts)
                {
                    if (post.GetAttribute("id") == lastPostId)
                    {
                        Console.WriteLine("Found last downvoted post and decided to stop");
                        break;
                    }

                    IWebElement downvoteButton = findNestedElment(".down", post, 5);

                    if (downvoteButton != null)
                    {
                        try
                        {
                            downvoteButton.Click();
                            lastPostId = post.GetAttribute("id");
                        }
                        catch (Exception error)
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

                Console.WriteLine("Going to sleep, see ya in 20 seconds");
                Thread.Sleep(20000);
            }
        }
    }
}