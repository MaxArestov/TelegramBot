using Telegram.Bot;
using Telegram.Bot.Types;
#nullable disable
var botClient = new TelegramBotClient("");
User bot = botClient.GetMeAsync().Result;

string pathSecretKey = "D:/Program/For teacher/TelegramBot/Try1/secretKey.txt";
string pathUserIds = "D:/Program/For teacher/TelegramBot/Try1/UserIds.txt";
string pathAdminIds = "D:/Program/For teacher/TelegramBot/Try1/AdminIds.txt";
string pathLogs = "D:/Program/For teacher/TelegramBot/Try1/Logs.txt";
List<long> userIds = new List<long>();
using StreamReader readerSecretKey = new StreamReader(pathSecretKey);
string secretKey = readerSecretKey.ReadToEnd();
readerSecretKey.Close();


List<long> adminIds = new List<long>();

Random random = new Random();
while (true)
{
    Update[] updates = await botClient.GetUpdatesAsync();
    for (int i = 0; i < updates.Length; i++)
    {
        using (StreamWriter writerLogs = new StreamWriter(pathLogs, true))
        {
            writerLogs.WriteLine($"{updates[i].Message.From.Id} ({updates[i].Message.From.FirstName} {updates[i].Message.From.LastName}): {updates[i].Message.Text}");
        }
        Console.WriteLine($"{updates[i].Message.From.Id} {updates[i].Message.From.FirstName} {updates[i].Message.From.LastName}: {updates[i].Message.Text}");
        if (!GetUserByID(updates[i].Message.From.Id))
        {
            AddUserToTxt(updates[i].Message.From.Id);
        }
        if (!CheckIdForAdmin(updates[i].Message.From.Id) && updates[i].Message.Text == secretKey)
        {
            SetAdmin(updates[i].Message.From.Id);
        }

        using StreamReader readerIdsOfAdmins = new StreamReader(pathAdminIds, System.Text.Encoding.Default);
        if (readerIdsOfAdmins.ReadToEnd().Contains(Convert.ToString(updates[i].Message.From.Id)))
        {
            readerIdsOfAdmins.Close();
            if (updates[i].Message.Text.Contains("CHANGE_PASSWORD"))
            {
                secretKey = updates[i].Message.Text.Remove(0, 16);
                SendMessageToAdminChangePassword(updates[i].Message);
                using (StreamWriter writerSecretKey = new StreamWriter(pathSecretKey, false)) // полная перезапись файла
                {
                    writerSecretKey.Write(secretKey.ToString());
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
            if (updates[i].Message.Text.Contains("everyone") && updates[i].Message.Text.Length > 9)
            {
                updates[i].Message.Text = updates[i].Message.Text.Remove(0, 9);
                SendMessageToEveryone(updates[i].Message);
            }
            if (updates[i].Message.Text.Contains("personal"))
            {
                SendToUser(updates[i]);
            }
            if (updates[i].Message.Text.Contains("MAKE_ADMIN"))
            {
                updates[i].Message.Text = updates[i].Message.Text.Remove(0, 11);
                if (!CheckIdForAdminAdd(updates[i].Message.Text))
                {
                    AddAdmin(updates[i].Message.Text);
                }
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
    using (StreamReader readerUserIdsFromTxt = new StreamReader(pathUserIds, System.Text.Encoding.Default))
    {
        string? lineUserIdsTxt;
        while ((lineUserIdsTxt = readerUserIdsFromTxt.ReadLine()) != null)
        {
            await botClient.SendTextMessageAsync(new ChatId(message.From.Id), lineUserIdsTxt);
        }
    }
}

async Task SendAdminIdsToAdmin(Message message)
{
    using (StreamReader readerAdminIdsFromTxt = new StreamReader(pathAdminIds, System.Text.Encoding.Default))
    {
        string? lineAdminIdsTxt;
        while ((lineAdminIdsTxt = readerAdminIdsFromTxt.ReadLine()) != null)
        {
            await botClient.SendTextMessageAsync(new ChatId(message.From.Id), lineAdminIdsTxt);
        }
        readerAdminIdsFromTxt.Close();
    }
}

bool CheckIdForAdmin(long userIdToAdmin)
{
    using (StreamReader readerAdminIds = new StreamReader(pathAdminIds, System.Text.Encoding.Default))
    {
        if (readerAdminIds.ReadToEnd().Contains(Convert.ToString(userIdToAdmin)))
        {
            readerAdminIds.Close();
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
bool CheckIdForAdminAdd(string messageNewAdminId)
{
    using StreamReader readerAdminIdsCheck = new StreamReader(pathAdminIds, System.Text.Encoding.Default);
    if (readerAdminIdsCheck.ReadToEnd().Contains(Convert.ToString(messageNewAdminId)))
    {
        readerAdminIdsCheck.Close();
        return true;
    }
    else
    {
        readerAdminIdsCheck.Close();
        return false;
    }
}
void AddAdmin(string mess)
{
    using StreamWriter writerAdminIdsAdd = new StreamWriter(pathAdminIds, true);
    writerAdminIdsAdd.WriteLine(mess);
    writerAdminIdsAdd.Close();
}
bool GetUserByID(long newId)
{
    using (StreamReader readerNewId = new StreamReader(pathUserIds, System.Text.Encoding.Default))
    {
        if (readerNewId.ReadToEnd().Contains(Convert.ToString(newId)))
        {
            readerNewId.Close();
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

async Task SendMessageToEveryone(Message message)
{
    using (StreamReader readerLinesOfUserIds = new StreamReader(pathUserIds))
    {
        string? lineuserIdsForMessage;
        while ((lineuserIdsForMessage = readerLinesOfUserIds.ReadLine()) != null)
        {
            await botClient.SendTextMessageAsync(new ChatId(lineuserIdsForMessage), message.Text);
        }
    }
}
async Task SendMessageToAdminChangePassword(Message mes)
{
    await botClient.SendTextMessageAsync(new ChatId(mes.From.Id), "Пароль успешно изменен.");
    await botClient.SendTextMessageAsync(new ChatId(mes.From.Id), $"Новый пароль - {secretKey}");
}
