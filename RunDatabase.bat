@echo off
echo ===================================================
echo  Starting Database Migration and Seeding...
echo ===================================================

dotnet ef migrations add UpdateCompanySeedData


dotnet ef database update

echo ===================================================
echo  Database Setup Completed Successfully!
echo ===================================================
pause