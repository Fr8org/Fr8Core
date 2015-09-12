using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Infrastructure.MultiTenant
{
    public class MT_Field : IMT_Field
    {

        public void Add(IUnitOfWork uow, Entities.MT_Field curMtField)
        {
            //int? mtFieldOffset = GetFieldColumnOffset(uow, curMtField.Name, curMtField.MT_ObjectId);
            Entities.MT_Field existingMtField = GetField(uow, curMtField.Name, curMtField.MT_ObjectId);

            if (existingMtField == null)
            {
                uow.MTFieldRepository.Add(curMtField);
                uow.SaveChanges();
            }
            else
            {
                if (existingMtField.FieldColumnOffset != curMtField.FieldColumnOffset)
                {
                    throw new DuplicateNameException(
                        string.Format("There is already a field with this set of values at offset value {0}",
                            existingMtField.FieldColumnOffset));
                }

                Update(uow, curMtField);
            }
        }

        public int? GetFieldColumnOffset(IUnitOfWork uow, string curMtFieldName, int curMtObjectId)
        {
            var mtObjectFields =
                uow.MTFieldRepository.GetQuery()
                    .Where(f => f.MT_ObjectId == curMtObjectId && f.Name.Equals(curMtFieldName))
                    .ToList();


            int numberOfFields = mtObjectFields.Count;

            //if more than one same MT Fields present for the MT Object, this is error
            if (numberOfFields > 1)
            {
                var curMtObject =
                    uow.MTObjectRepository.GetQuery()
                        .Include(obj => obj.MT_Organization)
                        .First(obj => obj.Id == curMtObjectId);

                throw new InvalidDataException(
                    string.Format("There exists more than one fields for the given Object {0} in Organization {1}.",
                        curMtObject.Name, curMtObject.MT_Organization.Name));
            }

            //if only one MT Field present for the given MT Object, return its offset
            if (numberOfFields == 1)
            {
                return mtObjectFields[0].FieldColumnOffset;
            }

            //if no MT Fields present for the given MT Object, return null
            return null;
        }

        public int GenerateFieldColumnOffset(IUnitOfWork uow, int curMtObjectId)
        {
            //get MT Fields of the given MT Object ID
            var mtFields = uow.MTFieldRepository.GetQuery().Where(f => f.MT_ObjectId == curMtObjectId).ToList();

            //if there are no MT Fields present for the given MT Object, return 1
            if (mtFields.Count == 0)
            {
                return 1;
            }

            //otherwise, calculate the maximum Offsetvalue of the fields and add 1
            return (mtFields.Max(f => f.FieldColumnOffset) + 1);
        }

        private void Update(IUnitOfWork uow, Entities.MT_Field curMtField)
        {
            var existingMtField = GetField(uow, curMtField.Name, curMtField.MT_ObjectId);

            //if there is an existing MT Field, Update their values with new MT Field
            if (existingMtField != null)
            {
                existingMtField.FieldColumnOffset = curMtField.FieldColumnOffset;
                existingMtField.MT_ObjectId = curMtField.MT_ObjectId;
                existingMtField.Name = curMtField.Name;
                existingMtField.Type = curMtField.Type;

                uow.SaveChanges();
            }
            else
            {
                //if there is no existing MT Field, this is error
                throw new InvalidOperationException(string.Format("Update Field is called on non-existing MT Field {0}", curMtField.Name));
            }
        }

        private Entities.MT_Field GetField(IUnitOfWork uow, string curMtFieldName, int curMtObjectId)
        {
            return
                uow.MTFieldRepository.FindOne(field => field.Name.Equals(curMtFieldName) && field.MT_ObjectId == curMtObjectId);
        }
    }
}
