using LoggingLibrary;
using SmartCacheGrains.Abstractions;
using SmartCacheGrains.State;

namespace SmartCacheGrains.Grains;

public class EmailDomainGrain : Grain, IEmailDomainGrain, IRemindable
{
    private readonly IPersistentState<EmailDomainGrainState> _state;

    public EmailDomainGrain(
        [PersistentState("emailDomainGrainState", "AzureBlobStorage")] IPersistentState<EmailDomainGrainState> state)
    {
        _state = state;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (_state.State.Emails == null)
        {
            _state.State.Emails = new List<string>();
        }

        await this.RegisterOrUpdateReminder("SaveGrainStateReminder", TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        SerilogLogger.LogGrainEvent(nameof(EmailDomainGrain), "Activated");

        await base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {

        var reminder = await this.GetReminder("SaveGrainStateReminder");
        if (reminder != null) 
        { 
            await this.UnregisterReminder(reminder);
        }
        await base.OnDeactivateAsync(reason, cancellationToken);
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName == "SaveGrainStateReminder")
        {
            var success = false;
            var retryCount = 0;
            const int maxRetries = 3;

            List<string> inMemoryEmails = new(_state.State.Emails);

            while (!success && retryCount < maxRetries)
            {
                retryCount++;

                try
                {
                    await _state.ReadStateAsync();

                    var mergedEmails = new HashSet<string>(_state.State.Emails);
                    mergedEmails.UnionWith(inMemoryEmails);

                    _state.State.Emails = mergedEmails.ToList();
                    await _state.WriteStateAsync();

                    success = true;
                    Console.WriteLine("[ReceiveReminder] Grain state merged and saved successfully.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ReceiveReminder] Grain state conflict detected, merge retry. {e}");
                    await Task.Delay(TimeSpan.FromMilliseconds(1000));
                }
            }

            if (!success)
            {
                Console.WriteLine($"[ReceiveReminder] Grain state conflict, merge not successful after {retryCount} retries.");
            }
        }
    }

    public async Task<bool> EmailExists(string email)
    {
        try
        {
            bool exist;
            if (_state.State.Emails.Contains(email))
            {
                exist = true;
                SerilogLogger.LogGrainEvent(nameof(EmailDomainGrain), $"Email {email} exists");
                return exist;
            }

            await _state.ReadStateAsync();

            var exists = _state.State.Emails.Contains(email);
            SerilogLogger.LogGrainEvent(nameof(EmailDomainGrain), $"Email {email} {(exists ? "exists" : "does not exist")}");
            return exists;
        }
        catch (Exception ex)
        {
            SerilogLogger.LogError(ex, $"Error checking email: {email}");
            throw;
        }
    }

    public async Task<bool> AddEmail(string email)
    {
        try
        {
            if (_state.State.Emails.Contains(email))
            {
                SerilogLogger.LogGrainEvent(nameof(EmailDomainGrain), $"Email {email} already stored.");
                return false;
            }

            _state.State.Emails.Add(email);
            SerilogLogger.LogGrainEvent(nameof(EmailDomainGrain), $"Email {email} added.");
            return true;
        }
        catch (Exception ex)
        {
            SerilogLogger.LogError(ex, $"Error adding email: {email}");
            throw;
        }
    }
}