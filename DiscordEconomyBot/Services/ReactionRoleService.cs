using Discord;
using Discord.WebSocket;
using DiscordEconomyBot.Data;
using DiscordEconomyBot.Models;

namespace DiscordEconomyBot.Services;

public class ReactionRoleService
{
    private readonly JsonDataStore _dataStore;
    private readonly DiscordSocketClient _client;

    public ReactionRoleService(JsonDataStore dataStore, DiscordSocketClient client)
    {
        _dataStore = dataStore;
        _client = client;

        // Subskrybuj wydarzenia reakcji
        _client.ReactionAdded += HandleReactionAdded;
        _client.ReactionRemoved += HandleReactionRemoved;
    }

    public async Task<(bool success, string message)> AddReactionRole(
        ulong messageId, 
        ulong channelId, 
        string emoji, 
        ulong roleId, 
        string roleName)
    {
        try
        {
            // Pobierz kana³ i wiadomoœæ
            var channel = _client.GetChannel(channelId) as IMessageChannel;
            if (channel == null)
            {
                return (false, "Nie znaleziono kana³u!");
            }

            var message = await channel.GetMessageAsync(messageId);
            if (message == null)
            {
                return (false, "Nie znaleziono wiadomoœci!");
            }

            // SprawdŸ czy reakcja ju¿ istnieje
            var existingReactions = _dataStore.GetAllReactionRoles();
            if (existingReactions.Any(r => r.MessageId == messageId && r.Emoji == emoji))
            {
                return (false, "Ta reakcja ju¿ jest przypisana do tej wiadomoœci!");
            }

            // Dodaj reakcjê do wiadomoœci
            IEmote emote;
            if (Emote.TryParse(emoji, out var customEmote))
            {
                emote = customEmote;
            }
            else
            {
                emote = new Emoji(emoji);
            }

            await message.AddReactionAsync(emote);

            // Zapisz konfiguracjê
            var reactionRole = new ReactionRole
            {
                MessageId = messageId,
                ChannelId = channelId,
                Emoji = emoji,
                RoleId = roleId,
                RoleName = roleName
            };

            _dataStore.AddReactionRole(reactionRole);

            return (true, $"Reaction Role dodany! U¿ytkownicy otrzymaj¹ rangê {roleName} po klikniêciu {emoji}");
        }
        catch (Exception ex)
        {
            return (false, $"B³¹d: {ex.Message}");
        }
    }

    public async Task<(bool success, string message)> RemoveReactionRole(ulong messageId, string emoji)
    {
        var reactionRoles = _dataStore.GetAllReactionRoles();
        var reactionRole = reactionRoles.FirstOrDefault(r => r.MessageId == messageId && r.Emoji == emoji);

        if (reactionRole == null)
        {
            return (false, "Nie znaleziono Reaction Role dla tej wiadomoœci i emoji!");
        }

        try
        {
            // Pobierz kana³ i wiadomoœæ
            var channel = _client.GetChannel(reactionRole.ChannelId) as IMessageChannel;
            if (channel != null)
            {
                var message = await channel.GetMessageAsync(messageId);
                if (message != null)
                {
                    // Usuñ reakcjê bota z wiadomoœci
                    IEmote emote;
                    if (Emote.TryParse(emoji, out var customEmote))
                    {
                        emote = customEmote;
                    }
                    else
                    {
                        emote = new Emoji(emoji);
                    }

                    await message.RemoveReactionAsync(emote, _client.CurrentUser);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Nie uda³o siê usun¹æ reakcji: {ex.Message}");
            // Kontynuuj mimo b³êdu - usuñ z bazy danych
        }

        _dataStore.RemoveReactionRole(messageId, emoji);
        return (true, $"Usuniêto Reaction Role dla {emoji} i usuniêto reakcjê z wiadomoœci");
    }

    public List<ReactionRole> GetAllReactionRoles()
    {
        return _dataStore.GetAllReactionRoles();
    }

    private async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> message, 
        Cacheable<IMessageChannel, ulong> channel, 
        SocketReaction reaction)
    {
        // Ignoruj reakcje od botów
        if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
            return;

        var reactionRoles = _dataStore.GetAllReactionRoles();
        var emojiString = reaction.Emote.ToString();

        var reactionRole = reactionRoles.FirstOrDefault(r => 
            r.MessageId == message.Id && r.Emoji == emojiString);

        if (reactionRole == null)
            return;

        // Pobierz u¿ytkownika i rangê
        var user = reaction.User.IsSpecified ? reaction.User.Value : null;
        if (user == null)
            return;

        var guildUser = user as SocketGuildUser;
        if (guildUser == null)
            return;

        var role = guildUser.Guild.GetRole(reactionRole.RoleId);
        if (role == null)
        {
            Console.WriteLine($"Nie znaleziono roli {reactionRole.RoleId}");
            return;
        }

        // Dodaj rangê u¿ytkownikowi
        if (!guildUser.Roles.Contains(role))
        {
            await guildUser.AddRoleAsync(role);
            Console.WriteLine($"Dodano rangê {role.Name} u¿ytkownikowi {guildUser.Username}");
        }
    }

    private async Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> message, 
        Cacheable<IMessageChannel, ulong> channel, 
        SocketReaction reaction)
    {
        // Ignoruj reakcje od botów
        if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
            return;

        var reactionRoles = _dataStore.GetAllReactionRoles();
        var emojiString = reaction.Emote.ToString();

        var reactionRole = reactionRoles.FirstOrDefault(r => 
            r.MessageId == message.Id && r.Emoji == emojiString);

        if (reactionRole == null)
            return;

        // Pobierz u¿ytkownika i rangê
        var user = reaction.User.IsSpecified ? reaction.User.Value : null;
        if (user == null)
            return;

        var guildUser = user as SocketGuildUser;
        if (guildUser == null)
            return;

        var role = guildUser.Guild.GetRole(reactionRole.RoleId);
        if (role == null)
            return;

        // Usuñ rangê u¿ytkownikowi
        if (guildUser.Roles.Contains(role))
        {
            await guildUser.RemoveRoleAsync(role);
            Console.WriteLine($"Usuniêto rangê {role.Name} u¿ytkownikowi {guildUser.Username}");
        }
    }
}
