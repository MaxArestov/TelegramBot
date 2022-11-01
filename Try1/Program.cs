using Telegram.Bot;
using Telegram.Bot.Types;
#nullable disable
var botClient = new TelegramBotClient("5731536747:AAFNBanq8ECXbR5Zer6arJh-F6FkPvNsiFw");
User bot = botClient.GetMeAsync().Result;

List<long> userIds = new List<long>();

string secretKey = "123321";

List<long> adminIds = new List<long>();

Random random = new Random();
while (true)
{
    Update[] updates = await botClient.GetUpdatesAsync();
    for (int i = 0; i < updates.Length; i++)
    {
        AddUser(updates[i].Message.From.Id);

        SetAdmin(updates[i].Message.Text, updates[i].Message.From.Id);

        if (adminIds.Contains(updates[i].Message.From.Id))
        {
            if (updates[i].Message.Text.Contains("CHANGE_PASSWORD"))
            {
                secretKey = updates[i].Message.Text.Remove(0, 15);
                SendMessageToAdminChangePassword(updates[i].Message);
            }
            if (updates[i].Message.Text == "GET")
            {
                SendUserIdsToAdmin(updates[i].Message);
            }
            if (updates[i].Message.Text.Contains("everyone"))
            {
                updates[i].Message.Text = updates[i].Message.Text.Remove(0, 8);
                SendMessageToEveryone(updates[i].Message);
            }
            if (updates[i].Message.Text.Contains("personal"))
            {
                SendToUser(updates[i]);
            }
        }
        updates = await botClient.GetUpdatesAsync(updates[^1].Id + 1);
    }
}

async Task SendToUser(Update update)
{
    string[] texts = update.Message.Text.Split(" ");
    bool isParsed = long.TryParse(texts[1], out long userId);

    if (isParsed)
    {
        string message = "";
        for (int o = 2; o < texts.Length; o++)
        {
            message += $"{texts[o]} ";
        }
        await botClient.SendTextMessageAsync(new ChatId(userId), message);
    }
}

async Task SendUserIdsToAdmin(Message message)
{
    for (int i = 0; i < userIds.Count; i++)
    {
        await botClient.SendTextMessageAsync(new ChatId(message.From.Id), userIds[i].ToString());
    }
}


void SetAdmin(string message, long userId)
{
    if (!adminIds.Contains(userId))
    {
        if (message == secretKey)
        {
            adminIds.Add(userId);
        }
    }
}

// void AddAdmin()
// {
//     adminIds.Add(userId);
// }

void AddUser(long userId)
{
    if (!userIds.Contains(userId))
    {
        userIds.Add(userId);
    }
}


async Task SendMessageToEveryone(Message message)
{
    for (int i = 0; i < userIds.Count; i++)
    {
        await botClient.SendTextMessageAsync(new ChatId(userIds[i]), message.Text);
    }
}
async Task SendMessageToAdminChangePassword(Message mes)
{
    for (int i = 0; i < adminIds.Count; i++)
    {
        await botClient.SendTextMessageAsync(new ChatId(mes.From.Id), "Пароль успешно изменен.");
        await botClient.SendTextMessageAsync(new ChatId(mes.From.Id), $"Новый пароль - {secretKey}");
    }
}