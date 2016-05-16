using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories.Authorization;

namespace Data.Repositories
{
    public class SqlAuthorizationTokenRepository : AuthorizationTokenRepositoryBase
    {
        /*********************************************************************************/
        // Declarations
        /*********************************************************************************/

        private readonly IDbSet<EncryptedAuthorizationData> _secureDBSet;

        /*********************************************************************************/
        // Functions
        /*********************************************************************************/

        public SqlAuthorizationTokenRepository(IUnitOfWork uow, IAuthorizationTokenStorageProvider storageProvider) 
            : base(storageProvider)
        {
            _secureDBSet = uow.Db.Set<EncryptedAuthorizationData>();
        }

        /*********************************************************************************/
        
        protected override void ProcessSecureDataChanges(IEnumerable<AuthorizationTokenDO> adds, IEnumerable<AuthorizationTokenDO> updates, IEnumerable<AuthorizationTokenDO> deletes)
        {
            foreach (var authorizationTokenDo in adds.Concat(updates))
            {
                var token = authorizationTokenDo;
                var item = _secureDBSet.FirstOrDefault(x => x.Id == token.Id);

                if (item != null)
                {
                    item.Data = authorizationTokenDo.Token;
                }
                else
                {
                    _secureDBSet.Add(new EncryptedAuthorizationData
                    {
                        Data = authorizationTokenDo.Token,
                        Id = authorizationTokenDo.Id
                    });
                }
            }

            foreach (var authorizationTokenDo in deletes)
            {
                var token = authorizationTokenDo;

                var item = _secureDBSet.FirstOrDefault(x => x.Id == token.Id);
                {
                    if (item != null)
                    {
                        _secureDBSet.Remove(item);
                    }
                }
            }
        }

        /*********************************************************************************/

        protected override string QuerySecurePart(Guid id)
        {
            var item = _secureDBSet.FirstOrDefault(x => x.Id == id);
            
            if (item == null)
            {
                return null;
            }

            return item.Data;
        }

        /*********************************************************************************/
    }
}
