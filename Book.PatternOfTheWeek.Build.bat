@echo on

REM visual studio vars expecially for msbuild
call "%VS120COMNTOOLS%"\vsvars32.bat

REM restore packages and create the tool chain
nuget restore src\MarkdownToEbook\MarkdownToEbook.sln -NonInteractive
msbuild src\MarkdownToEbook\MarkdownToEbook.sln /p:Configuration=Release

REM we need to change dir to the book to include the images correctly
cd "Book.PatternOfTheWeek"

..\bin\MarkdownToEbook.exe MergeFiles "." "*.md" "..\Released Books\PatternOfTheWeek.md"

..\bin\MarkdownToEbook.exe Markdown2Pdf "..\Released Books\PatternOfTheWeek.md" "..\Released Books\PatternOfTheWeek.pdf"

cd ..