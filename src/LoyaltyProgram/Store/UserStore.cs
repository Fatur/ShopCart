using LoyaltyProgram.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyProgram.Store
{
    public class UserStore : IUserStore
    {
        private static ConcurrentDictionary<int, LoyaltyProgramUser> m_userDb =
            new ConcurrentDictionary<int, LoyaltyProgramUser>();

        public LoyaltyProgramUser GetById(int id)
        {
            if (!m_userDb.ContainsKey(id))
                return null;
            return m_userDb[id];
        }

        public void Save(LoyaltyProgramUser newUser)
        {
                m_userDb[newUser.Id] = newUser;


        }
    }
}
