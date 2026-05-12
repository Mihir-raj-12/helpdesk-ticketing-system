using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.DTOs.Notifications
{
    public class NotificationPreferenceDto
    {
        public bool NotifyOnTicketCreated { get; set; } = true;
        public bool NotifyOnTicketAssigned { get; set; } = true;
        public bool NotifyOnStatusChanged { get; set; } = true;
        public bool NotifyOnNewComment { get; set; } = true;
        public bool NotifyOnTicketClosed { get; set; } = true;
        public bool OptOutCsatSurveys { get; set; } = false;
    }
}
