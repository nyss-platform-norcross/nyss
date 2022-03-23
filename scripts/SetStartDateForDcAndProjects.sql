DECLARE @StartDate DATE;
SET @StartDate = GETDATE() - 90;

UPDATE dc set dc.CreatedAt=@StartDate
FROM nyss.DataCollectors as dc

UPDATE p set p.StartDate=@StartDate
FROM nyss.Projects as p