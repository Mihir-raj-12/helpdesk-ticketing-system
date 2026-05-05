using HelpDesk.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories.Implementations.Service
{
    // The Interface so we can inject this into our API controllers later
    public interface IEmailQueue
    {
        ValueTask QueueEmailAsync(EmailPayload email);
        ValueTask<EmailPayload> DequeueEmailAsync(CancellationToken cancellationToken);
    }

    public class EmailQueue : IEmailQueue
    {
        private readonly Channel<EmailPayload> _queue;

        public EmailQueue()
        {
            // Unbounded means the waiting room can hold an infinite number of emails.
            // SingleReader = true makes it highly optimized because only our one Worker will read from it.
            var options = new UnboundedChannelOptions { SingleReader = true };
            _queue = Channel.CreateUnbounded<EmailPayload>(options);
        }

        // The API calls this to drop an email in the queue (Takes 0.01 milliseconds)
        public async ValueTask QueueEmailAsync(EmailPayload email)
        {
            await _queue.Writer.WriteAsync(email);
        }

        // The Worker calls this to wait for an email to arrive
        public async ValueTask<EmailPayload> DequeueEmailAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
