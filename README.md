# ContainsExtension

One of the many advantages of using a tool like Entity Framework Core is, that we are sure that the framework will generate properly parameterized SQL for us. This helps avoid SQL injection issues and avoids plan cache pollution. Unfortunately, EF Core currently falls short on that promise, when translating queries, where we supply a list of values to be matched against a column - Enumerable.Contains method - this is translated to a SQL Server IN operator

For example

```sql
SELECT [p].[Id], [p].[Brand], [p].[Model]
FROM [Phones] AS [p]
WHERE [p].[Id] IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25)
```
If SQL statements are not parameterized, we can end up with so many plans in the query cache, that on a busy system the database engine will spend all it's time evicting plans from the cache, instead of delivering query results.

![Before](https://i.imgur.com/8G2RcfP.png)

We can see that cached plans are not reused

Using **In** extension method

``` C#
              var result = db.Phones
                        .In(ids, d => d.Id)
                        .ToList();
````
SQL qyery will be 

```sql
exec sp_executesql N'SELECT [p].[Id], [p].[Brand], [p].[Model]
FROM [Phones] AS [p]
WHERE ((((([p].[Id] = @__v_0) OR ([p].[Id] = @__v_1)) OR (([p].[Id] = @__v_2) OR ([p].[Id] = @__v_3))) OR ((([p].[Id] = @__v_4) OR ([p].[Id] = @__v_5)) OR (([p].[Id] = @__v_6) OR ([p].[Id] = @__v_7)))) OR (((([p].[Id] = @__v_8) OR ([p].[Id] = @__v_9)) OR (([p].[Id] = @__v_10) OR ([p].[Id] = @__v_11))) OR ((([p].[Id] = @__v_12) OR ([p].[Id] = @__v_13)) OR (([p].[Id] = @__v_14) OR ([p].[Id] = @__v_15))))) OR ((((([p].[Id] = @__v_16) OR ([p].[Id] = @__v_17)) OR (([p].[Id] = @__v_18) OR ([p].[Id] = @__v_19))) OR ((([p].[Id] = @__v_20) OR ([p].[Id] = @__v_21)) OR (([p].[Id] = @__v_22) OR ([p].[Id] = @__v_23)))) OR (((([p].[Id] = @__v_24) OR ([p].[Id] = @__v_25)) OR (([p].[Id] = @__v_26) OR ([p].[Id] = @__v_27))) OR ((([p].[Id] = @__v_28) OR ([p].[Id] = @__v_29)) OR (([p].[Id] = @__v_30) OR ([p].[Id] = @__v_31)))))',N'@__v_0 int,@__v_1 int,@__v_2 int,@__v_3 int,@__v_4 int,@__v_5 int,@__v_6 int,@__v_7 int,@__v_8 int,@__v_9 int,@__v_10 int,@__v_11 int,@__v_12 int,@__v_13 int,@__v_14 int,@__v_15 int,@__v_16 int,@__v_17 int,@__v_18 int,@__v_19 int,@__v_20 int,@__v_21 int,@__v_22 int,@__v_23 int,@__v_24 int,@__v_25 int,@__v_26 int,@__v_27 int,@__v_28 int,@__v_29 int,@__v_30 int,@__v_31 int',@__v_0=1,@__v_1=2,@__v_2=3,@__v_3=4,@__v_4=5,@__v_5=6,@__v_6=7,@__v_7=8,@__v_8=9,@__v_9=10,@__v_10=11,@__v_11=12,@__v_12=13,@__v_13=14,@__v_14=15,@__v_15=16,@__v_16=17,@__v_17=18,@__v_18=19,@__v_19=20,@__v_20=21,@__v_21=22,@__v_22=23,@__v_23=24,@__v_24=25,@__v_25=25,@__v_26=25,@__v_27=25,@__v_28=25,@__v_29=25,@__v_30=25,@__v_31=25
```

And cached plan will be reused
![After](https://i.imgur.com/t4VGXBg.png)

SQL script to get cached execution plans
``` sql
SELECT cplan.usecounts, cplan.objtype, qtext.text, qplan.query_plan
FROM sys.dm_exec_cached_plans AS cplan
CROSS APPLY sys.dm_exec_sql_text(plan_handle) AS qtext
CROSS APPLY sys.dm_exec_query_plan(plan_handle) AS qplan
ORDER BY cplan.usecounts DESC
```
