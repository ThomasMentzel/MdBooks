@echo off

cd "Book.PatternOfTheWeek"

..\bin\MarkdownToEbook.exe MergeFiles "." "*.md" "..\Released Books\PatternOfTheWeek.md"

..\bin\MarkdownToEbook.exe Markdown2Pdf "..\Released Books\PatternOfTheWeek.md" "..\Released Books\PatternOfTheWeek.pdf"
