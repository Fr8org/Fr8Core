select count(p.Id) from Plans p 
	inner join PlanNodes pn on p.Id = pn.Id 
	inner join AspNetUsers u on pn.Fr8AccountId = u.Id 
where u.UserName IN ('integration_test_runner@fr8.company', 'fr8.madse.testing@gmail.com') and 
p.PlanState = 2