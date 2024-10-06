using System;
using System.ComponentModel.DataAnnotations;


namespace postiful.Enums
{
	public enum PinCreationEnum
	{
        [Display(Name = "Create Pin For Ad")]
        CreatePinForAd = 1,
        [Display(Name = "Create Pin For Idea")]
        CreatePinForIdea = 2,
        [Display(Name = "Create Pin")]
        CreatePinOrganic = 3
    }
}