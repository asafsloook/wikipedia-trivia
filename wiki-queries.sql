

select COUNT(readId)
from ReadingP
where userId = 193

select Score
from UsersP
where userId = 193

select count(CategoryName), CategoryName  from ReadCateogriesView  where userId=193  group by CategoryName  order by count(CategoryName) desc



select ArticleId, ReadingTime
from ReadingP
where userId = 193
order by ReadingTime desc

