update Plans set PlanState = 1 where Id in (
select p.Id from Plans p 
	inner join PlanNodes pn on p.Id = pn.Id 
	inner join AspNetUsers u on pn.Fr8AccountId = u.Id 
where u.UserName='integration_test_runner@fr8.company' and 
p.PlanState = 2)