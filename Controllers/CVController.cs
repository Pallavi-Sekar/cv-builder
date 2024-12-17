using Microsoft.AspNetCore.Mvc;
using CVBuilder.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System.Security.Claims;

namespace CVBuilder.Controllers
{
    public class CVController : Controller
    {
        private readonly CvBuilderContext _context;
        private readonly ILogger<CVController> _logger;

        public CVController(CvBuilderContext context, ILogger<CVController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Create CV - GET
        public IActionResult CreateCV()
        {
            return View();
        }

        // Create CV - POST
        [HttpPost]
        public async Task<IActionResult> CreateCV(CVViewModel model)
        {
            Console.WriteLine("Model ==== " + model);
            Console.WriteLine("PersonalDeatils ===== " + model.PersonalDetails);
            Console.WriteLine("PersonalDetails.FullName =======" + model.PersonalDetails.FullName);
            Console.WriteLine("WorkExperience ===== " + model.WorkExperiences);
            Console.WriteLine("WorkExperience[0].JobDescription ===== " + model.WorkExperiences[0].JobDescription);
            Console.WriteLine("ModelState.IsValid ======== " + ModelState.IsValid);
            Console.WriteLine("ModelState.IsValid ======== " + model.Educations[0]);
            Console.WriteLine("ModelState.IsValid ======== " + model.Educations[0].Institution);
            Console.WriteLine("ModelState.IsValid ======== " + model.Educations[0].Degree);
            Console.WriteLine("ModelState.IsValid ======== " + model.Educations[0].GraduationYear);
            if (ModelState.IsValid)
            {

                if (Request.Form["saveDraft"] == "true")
                {
                    // Save Draft logic
                    return await SaveDraft(model);  // Calling the SaveDraft method
                }
                try
                {
                    // Save Personal Details
                    await _context.PersonalDetails.AddAsync(model.PersonalDetails);
                    await _context.SaveChangesAsync();


                    Console.WriteLine("before workexperitnce");
                    // Save Work Experiences
                    foreach (var workExperience in model.WorkExperiences)
                    {
                        workExperience.PersonalDetailsId = model.PersonalDetails.Id;
                        await _context.WorkExperiences.AddAsync(workExperience);
                    }

                    Console.WriteLine("after workexperitnce");

                    // Save Education Details
                    foreach (var education in model.Educations)
                    {
                        education.PersonalDetailsId = model.PersonalDetails.Id;
                        await _context.Educations.AddAsync(education);
                    }

                    Console.WriteLine("before skill");

                    // Save Skills
                    foreach (var skill in model.Skills)
                    {
                        skill.PersonalDetailsId = model.PersonalDetails.Id;
                        await _context.Skills.AddAsync(skill);
                    }

                    Console.WriteLine("after skill");
                    // Save Languages
                    foreach (var language in model.Languages)
                    {
                        language.PersonalDetailsId = model.PersonalDetails.Id;
                        await _context.Languages.AddAsync(language);
                    }

                    Console.WriteLine("before save to db");

                    // Save changes to the database
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "CV created successfully!";
                    Console.WriteLine(model.WorkExperiences[0].CompanyName);
                    // Redirect based on selected template
                    if (model.SelectedTemplate == 1)
                    {
                        return RedirectToAction("ViewCV", new { id = model.PersonalDetails.Id });
                    }
                    else if (model.SelectedTemplate == 2)
                    {
                        return RedirectToAction("ViewCV2", new { id = model.PersonalDetails.Id });
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database update error.");
                    ModelState.AddModelError(string.Empty, "There was an issue saving your data. Please try again.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while saving the data.");
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                }
            }

            return View(model);
        }

        public IActionResult EditDraft(int id)
        {
            // Retrieve the draft from the database
            var draft = _context.PersonalDetails.FirstOrDefault(c => c.Id == id && c.IsDraft == true);

            if (draft == null)
            {
                return NotFound();  // Handle case where draft is not found
            }

            // Pass the draft to the EditDraft view
            return View(draft);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditDraft(int id, PersonalDetails updatedDraft)
        {
            if (id != updatedDraft.Id)
            {
                return BadRequest();  // Ensure the IDs match
            }

            // Retrieve the draft from the database
            var draft = _context.PersonalDetails.FirstOrDefault(c => c.Id == id && c.IsDraft == true);

            if (draft == null)
            {
                return NotFound();  // Handle case where draft is not found
            }

            // Update the draft's fields
            draft.FullName = updatedDraft.FullName;
            draft.Email = updatedDraft.Email;
            // You can add more fields as necessary

            // Save the changes to the database
            _context.SaveChanges();

            // Redirect to ViewDrafts page after saving
            return RedirectToAction("ViewDrafts");
        }

        public IActionResult ViewDrafts()
        {
            // Query the database for all drafts (IsDraft == true)
            var drafts = _context.PersonalDetails
                                 .Where(c => c.IsDraft == true)
                                 .ToList();


            Console.WriteLine("drafts[0].PersonalDetails.FullName ============ " + drafts[0].FullName);

            // Pass drafts to the view
            return View(drafts);
        }


        [HttpPost]
        public async Task<IActionResult> SaveDraft(CVViewModel model)
        {
            Console.WriteLine("ModelState.IsValid =========== " + ModelState.IsValid);
            if (ModelState.IsValid)
            {
                try
                {
                    model.IsDraft = 1;
                    model.PersonalDetails.IsDraft = true;
                    model.WorkExperiences.ForEach(w => w.IsDraft = true);
                    model.Educations.ForEach(e => e.IsDraft = true);
                    model.Skills.ForEach(s => s.IsDraft = true);
                    model.Languages.ForEach(l => l.IsDraft = true);
                    // Save Personal Details (draft)
                    if (model.PersonalDetails.Id == 0)
                    {
                        await _context.PersonalDetails.AddAsync(model.PersonalDetails);
                    }
                    else
                    {
                        _context.PersonalDetails.Update(model.PersonalDetails);
                    }
                    await _context.SaveChangesAsync();

                    // Save Work Experiences (draft)
                    foreach (var workExperience in model.WorkExperiences)
                    {
                        workExperience.PersonalDetailsId = model.PersonalDetails.Id;
                        if (workExperience.Id == 0)
                        {
                            await _context.WorkExperiences.AddAsync(workExperience);
                        }
                        else
                        {
                            _context.WorkExperiences.Update(workExperience);
                        }
                    }

                    // Save Education Details (draft)
                    foreach (var education in model.Educations)
                    {
                        education.PersonalDetailsId = model.PersonalDetails.Id;
                        if (education.Id == 0)
                        {
                            await _context.Educations.AddAsync(education);
                        }
                        else
                        {
                            _context.Educations.Update(education);
                        }
                    }

                    // Save Skills (draft)
                    foreach (var skill in model.Skills)
                    {
                        skill.PersonalDetailsId = model.PersonalDetails.Id;
                        if (skill.Id == 0)
                        {
                            await _context.Skills.AddAsync(skill);
                        }
                        else
                        {
                            _context.Skills.Update(skill);
                        }
                    }

                    // Save Languages (draft)
                    foreach (var language in model.Languages)
                    {
                        language.PersonalDetailsId = model.PersonalDetails.Id;
                        if (language.Id == 0)
                        {
                            await _context.Languages.AddAsync(language);
                        }
                        else
                        {
                            _context.Languages.Update(language);
                        }
                    }

                    // Save changes to the database for draft
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Your draft has been saved successfully!";
                    return RedirectToAction("ViewDrafts");
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database update error while saving draft.");
                    ModelState.AddModelError(string.Empty, "There was an issue saving your draft. Please try again.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while saving the draft.");
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while saving your draft. Please try again.");
                }
            }

            return View("CreateCV", model); // You can return the user to the CreateCV view or a new draft view
        }


        // View CV - GET
        public async Task<IActionResult> ViewCV(int id)
        {
            // Fetch Personal Details
            var personalDetails = await _context.PersonalDetails
                .FirstOrDefaultAsync(p => p.Id == id);

            if (personalDetails == null)
            {
                TempData["ErrorMessage"] = "The requested CV does not exist.";
                return RedirectToAction("CreateCV");
            }

            // Fetch Related Data
            var workExperiences = await _context.WorkExperiences
                .Where(w => w.PersonalDetailsId == id)
                .ToListAsync();
            Console.WriteLine("Work Experiences Count: " + workExperiences.Count);

            var educations = await _context.Educations
                .Where(e => e.PersonalDetailsId == id)
                .ToListAsync();
            Console.WriteLine("Educations Count: " + educations.Count);

            var skills = await _context.Skills
                .Where(s => s.PersonalDetailsId == id)
                .ToListAsync();
            Console.WriteLine("Skills Count: " + skills.Count);

            var languages = await _context.Languages
                .Where(l => l.PersonalDetailsId == id)
                .ToListAsync();
            Console.WriteLine("Languages Count: " + languages.Count);


            // Map data to CVViewModel
            var model = new CVViewModel
            {
                PersonalDetails = personalDetails,
                WorkExperiences = workExperiences,
                Educations = educations,
                Skills = skills,
                Languages = languages
            };

            return View(model);
        }

        // View CV 2 - GET
        public async Task<IActionResult> ViewCV2(int id)
        {
            // Fetch Personal Details
            var personalDetails = await _context.PersonalDetails
                .FirstOrDefaultAsync(p => p.Id == id);

            if (personalDetails == null)
            {
                TempData["ErrorMessage"] = "The requested CV does not exist.";
                return RedirectToAction("CreateCV");
            }

            // Fetch Related Data
            var workExperiences = await _context.WorkExperiences
                .Where(w => w.PersonalDetailsId == id)
                .OrderByDescending(w => w.StartDate) // Assuming ViewCV2 orders by StartDate
                .ToListAsync();
            Console.WriteLine("Work Experiences Count: " + workExperiences.Count);

            var educations = await _context.Educations
                .Where(e => e.PersonalDetailsId == id)
                .OrderByDescending(e => e.GraduationYear) // Order by most recent education
                .ToListAsync();
            Console.WriteLine("Educations Count: " + educations.Count);

            var skills = await _context.Skills
                .Where(s => s.PersonalDetailsId == id)
                .OrderBy(s => s.SkillName) // Alphabetical order for better readability
                .ToListAsync();
            Console.WriteLine("Skills Count: " + skills.Count);

            var languages = await _context.Languages
                .Where(l => l.PersonalDetailsId == id)
                .OrderBy(l => l.LanguageName) // Alphabetical order
                .ToListAsync();
            Console.WriteLine("Languages Count: " + languages.Count);

            // Map data to an enhanced CVViewModel (specific to ViewCV2)
            var model = new CVViewModel
            {
                PersonalDetails = personalDetails,
                WorkExperiences = workExperiences,
                Educations = educations,
                Skills = skills,
                Languages = languages,
            };

            return View("ViewCV2", model); // Ensure "ViewCV2" view exists
        }


        // Generate PDF
        [HttpPost]
        public async Task<IActionResult> GeneratePDF(int id)
        {
            // Fetch CV data
            QuestPDF.Settings.License = LicenseType.Community;
            var personalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(p => p.Id == id);
            if (personalDetails == null) return NotFound("The requested CV does not exist.");

            var workExperiences = await _context.WorkExperiences.Where(w => w.PersonalDetailsId == id).ToListAsync();
            var educations = await _context.Educations.Where(e => e.PersonalDetailsId == id).ToListAsync();
            var skills = await _context.Skills.Where(s => s.PersonalDetailsId == id).ToListAsync();
            var languages = await _context.Languages.Where(l => l.PersonalDetailsId == id).ToListAsync();

            // Generate PDF using QuestPDF
            var document = Document.Create(container =>
{
    container.Page(page =>
    {
        page.Margin(50);

        // Header with highlighted background
        page.Header().Background(Colors.Blue.Lighten2).Padding(10).Row(row =>
        {
            row.RelativeItem().AlignLeft().Text("Curriculum Vitae")
                .FontSize(28)
                .Bold()
                .FontColor(Colors.White);
        });

        page.Content().Column(column =>
        {
            // Personal Information Section
            column.Item().Text("Personal Information").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
            column.Item().LineHorizontal(1).LineColor(Colors.Blue.Lighten3);
            column.Item().Row(row =>
            {
                row.RelativeItem(2).Text("Name").Bold().FontColor(Colors.Grey.Darken3);
                row.RelativeItem(8).Text($"{personalDetails.FullName}");
            });
            column.Item().Row(row =>
            {
                row.RelativeItem(2).Text("Contact").Bold().FontColor(Colors.Grey.Darken3);
                row.RelativeItem(8).Text($"{personalDetails.ContactNumber}");
            });
            column.Item().Row(row =>
            {
                row.RelativeItem(2).Text("Email").Bold().FontColor(Colors.Grey.Darken3);
                row.RelativeItem(8).Text($"{personalDetails.Email}");
            });
            column.Item().Row(row =>
            {
                row.RelativeItem(2).Text("Address").Bold().FontColor(Colors.Grey.Darken3);
                row.RelativeItem(8).Text($"{personalDetails.Address}");
            });

            column.Item().LineHorizontal(1).LineColor(Colors.Blue.Lighten3);

            // Work Experience Section
            column.Item().Text("Work Experience").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
            column.Item().LineHorizontal(1).LineColor(Colors.Blue.Lighten3);
            foreach (var work in workExperiences)
            {
                column.Item().Text($"{work.JobTitle} at {work.CompanyName}")
                    .FontSize(16)
                    .Bold()
                    .FontColor(Colors.Blue.Darken1);
                column.Item().Text($"{work.StartDate} - {work.EndDate ?? "Present"}")
                    .FontSize(12)
                    .Italic()
                    .FontColor(Colors.Grey.Darken1);
                column.Item().Text($"{work.JobDescription}")
                    .FontSize(12)
                    .FontColor(Colors.Black);
            }

            column.Item().LineHorizontal(1).LineColor(Colors.Blue.Lighten3);

            // Education Section
            column.Item().Text("Education").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
            column.Item().LineHorizontal(1).LineColor(Colors.Blue.Lighten3);
            foreach (var education in educations)
            {
                column.Item().Text($"{education.Degree}, {education.Institution}")
                    .FontSize(16)
                    .Bold()
                    .FontColor(Colors.Blue.Darken1);
                column.Item().Text($"Graduation Year: {education.GraduationYear}")
                    .FontSize(12)
                    .Italic()
                    .FontColor(Colors.Grey.Darken1);
            }

            column.Item().LineHorizontal(1).LineColor(Colors.Blue.Lighten3);

            // Skills Section
            column.Item().Text("Skills").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
            column.Item().LineHorizontal(1).LineColor(Colors.Blue.Lighten3);
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    foreach (var skill in skills)
                    {
                        column.Item().Text($"• {skill.SkillName}")
                            .FontSize(12)
                            .FontColor(Colors.Grey.Darken3);
                    }
                });
            });

            column.Item().LineHorizontal(1).LineColor(Colors.Blue.Lighten3);

            // Languages Section
            column.Item().Text("Languages").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
            column.Item().LineHorizontal(1).LineColor(Colors.Blue.Lighten3);
            foreach (var language in languages)
            {
                column.Item().Text($"- {language.LanguageName}")
                    .FontSize(12)
                    .FontColor(Colors.Grey.Darken3);
            }
        });

        // Footer with contact info
        page.Footer().Padding(10).Background(Colors.Blue.Lighten2).Row(row =>
        {
            row.RelativeItem().AlignCenter().Text("Generated by CV Builder")
                .FontSize(10)
                .FontColor(Colors.White);
        });
    });
});


            // Generate the PDF file
            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", "CV1.pdf");
        }



        public async Task<IActionResult> GenerateCVPdf(int id)
        {
            // Fetch CV data
            QuestPDF.Settings.License = LicenseType.Community;
            var personalDetails = await _context.PersonalDetails.FirstOrDefaultAsync(p => p.Id == id);
            if (personalDetails == null) return NotFound("The requested CV does not exist.");

            var workExperiences = await _context.WorkExperiences.Where(w => w.PersonalDetailsId == id).ToListAsync();
            var educations = await _context.Educations.Where(e => e.PersonalDetailsId == id).ToListAsync();
            var skills = await _context.Skills.Where(s => s.PersonalDetailsId == id).ToListAsync();
            var languages = await _context.Languages.Where(l => l.PersonalDetailsId == id).ToListAsync();
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Inch);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));
                    page.Content().Column(column =>
                    {
                        // Header Section
                        column.Item().Text(personalDetails.FullName)
                            .FontSize(20).Bold().AlignCenter();
                        column.Item().Text($"{personalDetails.Email} | {personalDetails.ContactNumber}")
                            .FontSize(12).AlignCenter();
                        column.Item().Text(" ");

                        // Personal Information Section
                        column.Item().Text("Personal Information").FontSize(16).Bold().Underline();
                        column.Item().Text($"Address: {personalDetails.Address}");
                        column.Item().Text(" ");

                        // Work Experience Section
                        column.Item().Text("Work Experience").FontSize(16).Bold().Underline();
                        if (workExperiences.Any())
                        {
                            foreach (var work in workExperiences)
                            {
                                column.Item().Text($"- {work.CompanyName}").Bold();
                                column.Item().Text($"  Role: {work.JobTitle}");
                                column.Item().Text($"  Duration: {work.StartDate} - {work.EndDate}");
                                column.Item().Text($"  Description: {work.JobDescription}");
                            }
                        }
                        else
                        {
                            column.Item().Text("No work experience provided.");
                        }
                        column.Item().Text(" ");

                        // Education Section
                        column.Item().Text("Education").FontSize(16).Bold().Underline();
                        if (educations.Any())
                        {
                            foreach (var education in educations)
                            {
                                column.Item().Text($"- {education.Institution}").Bold();
                                column.Item().Text($"  Degree: {education.Degree}");
                                column.Item().Text($"  Graduation Year: {education.GraduationYear}");
                            }
                        }
                        else
                        {
                            column.Item().Text("No education details provided.");
                        }
                        column.Item().Text(" ");

                        // Skills Section
                        column.Item().Text("Skills").FontSize(16).Bold().Underline();
                        if (skills.Any())
                        {
                            column.Item().Row(row =>
                            {
                                foreach (var skill in skills)
                                {
                                    row.RelativeItem().Background("#e0e0e0").Padding(5).Text(skill.SkillName)
                                        .FontSize(12);
                                }
                            });
                        }
                        else
                        {
                            column.Item().Text("No skills listed.");
                        }
                        column.Item().Text(" ");

                        // Languages Section
                        column.Item().Text("Languages").FontSize(16).Bold().Underline();
                        if (languages.Any())
                        {
                            foreach (var language in languages)
                            {
                                column.Item().Text($"- {language.LanguageName}");
                            }
                        }
                        else
                        {
                            column.Item().Text("No languages listed.");
                        }
                    });

                    // Footer Section
                    page.Footer().AlignCenter().Text($"© {DateTime.Now.Year} CV Builder - Template 2")
                        .FontSize(10).Italic();
                });
            }).GeneratePdf();
            return File(pdfBytes, "application/pdf", "CV2.pdf");
        }




    }
}
