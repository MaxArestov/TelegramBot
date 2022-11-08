using Telegram.Bot;
using Telegram.Bot.Types;
#nullable disable
var botClient = new TelegramBotClient("5731536747:AAFNBanq8ECXbR5Zer6arJh-F6FkPvNsiFw");
User bot = botClient.GetMeAsync().Result;

string pathSecretKey = "D:/Program/For teacher/TelegramBot/Try1/secretKey.txt";
string pathUserIds = "D:/Program/For teacher/TelegramBot/Try1/UserIds.txt";
string pathAdminIds = "D:/Program/For teacher/TelegramBot/Try1/AdminIds.txt";
List<long> userIds = new List<long>();
using StreamReader readerSecretKey = new StreamReader(pathSecretKey);
string secretKey = readerSecretKey.ReadLine();
readerSecretKey.Close();



List<long> adminIds = new List<long>();

Random random = new Random();
while (true)
{
    Update[] updates = await botClient.GetUpdatesAsync();
    for (int i = 0; i < updates.Length; i++)
    {
        if (!GetUserByID(updates[i].Message.From.Id))
        {
            AddUserToTxt(updates[i].Message.From.Id);
        }
        if (!CheckIdForAdmin(updates[i].Message.From.Id) && updates[i].Message.Text == secretKey)
        {
            SetAdmin(updates[i].Message.From.Id);
        }


        if (adminIds.Contains(updates[i].Message.From.Id))
        {
            if (updates[i].Message.Text.Contains("CHANGE_PASSWORD"))
            {
                secretKey = updates[i].Message.Text.Remove(0, 16);
                SendMessageToAdminChangePassword(updates[i].Message);
                using (StreamWriter writerSecretKey = new StreamWriter(pathSecretKey, false)) // полная перезапись файла
                {
                    writerSecretKey.Write($"{secretKey}");
                    writerSecretKey.Close();
                }
            }
            if (updates[i].Message.Text == "GET")
            {
                SendUserIdsToAdmin(updates[i].Message);
            }
            if (updates[i].Message.Text == "GETADMINS")
            {
                SendAdminIdsToAdmin(updates[i].Message);
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
            if (updates[i].Message.Text.Contains("MAKE_ADMIN"))
            {
                updates[i].Message.Text = updates[i].Message.Text.Remove(0, 10);
                AddAdmin(updates[i].Message);
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
        using (StreamReader readerUserIdsFromTxt = new StreamReader(pathUserIds, System.Text.Encoding.Default))
        {
            string? lineUserIdsTxt;
            while ((lineUserIdsTxt = readerUserIdsFromTxt.ReadLine()) != null)
            {
                await botClient.SendTextMessageAsync(new ChatId(message.From.Id), userIds[i].ToString());
            }
        }
    }
}

async Task SendAdminIdsToAdmin(Message message)
{
    for (int i = 0; i < userIds.Count; i++)
    {
        await botClient.SendTextMessageAsync(new ChatId(message.From.Id), adminIds[i].ToString());
    }
}

bool CheckIdForAdmin(long userIdToAdmin)
{
    using (StreamReader readerAdminIds = new StreamReader(pathAdminIds, System.Text.Encoding.Default))
    {
        if (readerAdminIds.ReadToEnd().Contains(Convert.ToString(userIdToAdmin)))
        {
            return true;
        }
        else return false;
    }
}
void SetAdmin(long newAdminId)
{
    using (StreamWriter writerNewAdminId = new StreamWriter(pathAdminIds, true))
    {
        writerNewAdminId.WriteLine(Convert.ToString(newAdminId));
        writerNewAdminId.Close();
    }
}

void AddAdmin(Message mess)
{
    string newIdToAdmin = mess.Text;
    long messlong = 0;
    bool isParsedLong = long.TryParse(newIdToAdmin, out messlong);
    if (isParsedLong)
    {
        if (userIds.Contains(messlong))
        {
            adminIds.Add(messlong);
        }
    }
}
bool GetUserByID(long newId)
{
    using (StreamReader readerNewId = new StreamReader(pathUserIds, System.Text.Encoding.Default))
    {
        if (readerNewId.ReadToEnd().Contains(Convert.ToString(newId)))
        {
            return true;
        }
        else return false;
    }
}
void AddUserToTxt(long newUserId)
{
    using StreamWriter writerNewId = new StreamWriter(pathUserIds, true);
    writerNewId.WriteLine($"{newUserId}");
    writerNewId.Close();
}
void AddUser()
{
    using (StreamReader readerUserIds = new StreamReader(pathUserIds, System.Text.Encoding.Default))
    {
        string lineUserIds;
        while ((lineUserIds = readerUserIds.ReadLine()) != null)
        {
            if (!userIds.Contains(long.Parse(lineUserIds)))
            {
                userIds.Add(long.Parse(lineUserIds));
            }
        }
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
