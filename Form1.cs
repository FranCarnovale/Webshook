using System;
using Discord;
using Discord.Webhook;
using System.Windows.Forms;
using System.Net;
using System.IO;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using Windows.UI.Xaml.Shapes;

namespace Webshook
{
    public partial class Form1 : Form
    {
        public string url;
        public int amount;
        public DiscordWebhook hook = new DiscordWebhook();

        public Form1()
        {
            InitializeComponent();
        }

        #region Utils
        private bool IsValidWebhook(string webhook)
        {
            try
            {
                HttpWebRequest r = (HttpWebRequest)WebRequest.Create(webhook);
                HttpWebResponse wr = (HttpWebResponse)r.GetResponse();
                StreamReader sr = new StreamReader(wr.GetResponseStream());

                if (!sr.ReadToEnd().Contains("\"message\": \"Invalid Webhook Token\""))
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private int ValidateAmount()
        {
            if (String.IsNullOrEmpty(amountBox.Text))
            {
                MessageBox.Show("Please enter an amount");
                return 0;
            }
            amount = int.Parse(amountBox.Text);
            if (!int.TryParse(amountBox.Text, out amount))
            {
                MessageBox.Show("Enter a valid number.");
            }
            return amount;
        }


        #endregion

        #region On Click Events
        private void spamButton_Click(object sender, EventArgs e)
        {
            StartSpam(webhookBox.Text);
        }

        private void silentButton_Click(object sender, EventArgs e)
        {
            if (delHookBox.Text != "")
            {
                DeleteWebhook(delHookBox.Text, true);
            }
            else
            {
                MessageBox.Show("Enter in a webhook.");
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (delHookBox.Text != "")
            {
                DeleteWebhook(delHookBox.Text, false);
            }
            else
            {
                MessageBox.Show("Enter in a webhook.");
            }
        }
        private void loadHooksBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Title = "Select the file with Webhooksw";
            open.Filter = "Text files | *.txt";
            open.InitialDirectory = Directory.GetCurrentDirectory();

            if (open.ShowDialog() == DialogResult.OK)
            {
                string name = open.FileName;
                string[] lines = File.ReadAllLines(name);
                LoadHooks(lines);
            }

        }
        private void resetWebhooks_Click(object sender, EventArgs e)
        {
            hookList.DataSource = null;
            hookList.Items.Clear();
            webhooksLoadedLabel.Text = "0";
            new ToastContentBuilder()
                .AddText("Webshook")
                .AddText("Webhook box has been reset!")
                .Show();
        }

        private void multiSpamBtn_Click(object sender, EventArgs e)
        {
            if (hookList.Items.Count <= 1)
            {
                MessageBox.Show("Put more than one webhook, or just use single-hook.");
            }
            else
            {
                if (ValidateAmount() > 0)
                {
                    Parallel.ForEach(hookList.Items.Cast<string>().ToList(), hook =>
                    {
                        StartSpam(hook);
                    });
                    new ToastContentBuilder()
                        .AddText("Webshook")
                        .AddText("Webhooks have been spammed :)")
                        .Show();
                }
            }
        }

        private void multiSilentBtn_Click(object sender, EventArgs e)
        {
            if (hookList.Items.Count <= 1)
            {
                MessageBox.Show("Enter more than one webhook, or just use single-hook.");
            }
            else
            {
                foreach (string hook in hookList.Items)
                {
                    try
                    {
                        DeleteWebhook(hook, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                new ToastContentBuilder()
                    .AddText("Webshook")
                    .AddText("Webhooks have been silently deleted ;)")
                    .Show();
            }
        }

        private void multiDeleteBtn_Click(object sender, EventArgs e)
        {
            if (hookList.Items.Count <= 1)
            {
                MessageBox.Show("Enter more than one webhook, or just use the single-hook.");
            }
            else
            {
                foreach (string hook in hookList.Items)
                {
                    try
                    {
                        DeleteWebhook(hook, false);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                new ToastContentBuilder()
                    .AddText("Webshook")
                    .AddText("Webhooks have been deleted ;)")
                    .Show();
            }
        }

        private void embedSpamBtn_Click(object sender, EventArgs e)
        {
            EmbedSpam(webhookBox.Text);
        }

        private void embedMultiSpam_Click(object sender, EventArgs e)
        {
            if (hookList.Items.Count <= 1)
            {
                MessageBox.Show("Enter more than one webhook, or just use the single-hook.");
            }
            else
            {
                Parallel.ForEach(hookList.Items.Cast<string>().ToList(), hook =>
                {
                    EmbedSpam(hook);
                });
                new ToastContentBuilder()
                    .AddText("Webshook")
                    .AddText("Webhooks have been spammed :)")
                    .Show();
            }
        }
        #endregion

        #region Multi-Hook Check
        private void LoadHooks(string[] lines)
        {
            List<string> valid = new List<string>();
            
            foreach (string line in lines)
            {
                if (line.Contains("https://discord.com/api/webhooks/"))
                {
                    if (IsValidWebhook(line))
                        valid.Add(line);
                }
            }
            hookList.DataSource = valid;
            webhooksLoadedLabel.Text = hookList.Items.Count.ToString();
            new ToastContentBuilder()
                .AddText("Webshook")
                .AddText($"{valid.Count()} Webhooks Loaded!")
                .Show();
        }
        #endregion
        #region Delete Methods
        public async void DeleteWebhook(string url, bool silent)
        {
            if (IsValidWebhook(url))
            {
                if (!silent)
                {
                    hook.Url = url;
                    DiscordMessage message = new DiscordMessage
                    {
                        Content = "Say goodbye to your webhook!",
                        Username = "Bye bye :3",
                        AvatarUrl = avatarBox.Text
                    };
                    hook.Send(message);
                    await Task.Delay(1000);
                }
                MessageBox.Show(Delete(url));
            }
            else
            {
                MessageBox.Show("Invalid webhook");
            }
        }

        public string Delete(string webhook)
        {
            try
            {
                HttpWebRequest r = (HttpWebRequest)WebRequest.Create(webhook);
                r.Method = "DELETE";
                HttpWebResponse wr = (HttpWebResponse)r.GetResponse();
                new ToastContentBuilder()
                    .AddText("Webshook")
                    .AddText("Webhook has been deleted!")
                    .Show();
                var sr = new StreamReader(wr.GetResponseStream()).ReadToEnd();
                if (String.IsNullOrEmpty(sr))
                    sr = "Webhook deleted!";
                return sr;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Invalid"))
                {
                    new ToastContentBuilder()
                        .AddText("Webshook")
                        .AddText("Webhook is invalid.")
                        .Show();
                    return "Invalid";
                }
                else if (ex.Message.Contains("404"))
                {
                    new ToastContentBuilder()
                        .AddText("Webshook")
                        .AddText("Webhook doesn't exist.")
                        .Show();
                    return "Doesn't Exist";
                }
                else
                {
                    MessageBox.Show($"Error: {ex}");
                    return ex.ToString();
                }
            }
        }
        #endregion
        #region Spam Methods
        public async void RealSpam(DiscordWebhook hook, DiscordMessage msg)
        {
            for (int i = 0; i < amount; i++)
            {
                try
                {
                    hook.Send(msg);
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("429"))
                    {
                        if (ratelimitCheck.Checked)
                        {
                            new ToastContentBuilder()
                                .AddText("Webshook")
                                .AddText("You are being Ratelimited")
                                .Show();
                            await Task.Delay(5000);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Error: {e}");
                        break;
                    }
                }
            }
        }
        public void StartSpam(string webhook)
        {
            if (!IsValidWebhook(webhook))
            {
                MessageBox.Show("Invalid webhook");
                return;
            }
            DiscordWebhook hook = new DiscordWebhook();
            hook.Url = webhook;
            DiscordMessage message = new DiscordMessage();
            message.Content = messageBox.Text;
            message.Username = usernameBox.Text;
            message.AvatarUrl = avatarBox.Text;
            amount = ValidateAmount();
            if (amount > 0)
            {
                if (usernameBox.Text != "" || messageBox.Text != "")
                {
                    RealSpam(hook, message);
                }
                else
                {
                    MessageBox.Show("Please fill in all required fields (*)");
                }
            }
        }
        public void EmbedSpam(string webhook)
        {
            if (!IsValidWebhook(webhook))
            {
                MessageBox.Show("Invalid webhook");
                return;
            }
            DiscordWebhook hook = new DiscordWebhook();
            DiscordMessage msg = new DiscordMessage();
            hook.Url = webhook;
            DiscordEmbed emb = new DiscordEmbed
            {
                Title = titleText.Text,
                Description = descriptionText.Text,
                Image = new EmbedMedia() { Url = imageBox.Text },
                Thumbnail = new EmbedMedia() { Url = thumbnailBox.Text },
                Author = new EmbedAuthor() { Name = authorBox.Text },
                Footer = new EmbedFooter() { Text = footerBox.Text }
            };
            msg.Embeds = new[] { emb };
            if (String.IsNullOrEmpty(amountBox.Text))
            {
                MessageBox.Show("Please enter an amount");
                return;
            }
            amount = ValidateAmount();
            if (amount > 0)
            {
                if (titleText.Text != "" || descriptionText.Text != "" || authorBox.Text != "" || footerBox.Text != "")
                {
                    RealSpam(hook, msg);
                }
                else
                {
                    MessageBox.Show("Please fill in atleast one field.");
                }
            }
        }
        #endregion

        private bool mouseDown;
        private Point lastLoc;

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLoc = e.Location;
        }
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                Location = new Point((Location.X - lastLoc.X) + e.X, (Location.Y - lastLoc.Y) + e.Y);
                Update();
            }
        }
    }
}