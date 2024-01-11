using MinSheng_MIS.Models;
using MinSheng_MIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace MinSheng_MIS.Services
{
    public class MeetingMinutesService
    {
        Bimfm_MinSheng_MISEntities db = new Bimfm_MinSheng_MISEntities();

        public void AddMeetingMinutes(MeetingMinutesInfo Info, string MeetingFile, string UserName)
        {
            MeetingMinutes meetingMinutes = new MeetingMinutes();
            meetingMinutes.MMSN = Info.MMSN;
            meetingMinutes.MeetingTopic = Info.MeetingTopic;
            meetingMinutes.MeetingDate = Info.MeetingDate;
            meetingMinutes.MeetingDateStart = Info.MeetingDateStart;
            meetingMinutes.MeetingDateEnd = Info.MeetingDateEnd;
            meetingMinutes.MeetingVenue = Info.MeetingVenue;
            meetingMinutes.Chairperson = Info.Chairperson;
            meetingMinutes.Participant = Info.Participant;
            meetingMinutes.ExpectedAttendence = Info.ExpectedAttendence;
            meetingMinutes.ActualAttendence = Info.ActualAttendence;
            meetingMinutes.AbsenteeList = Info.AbsenteeList;
            meetingMinutes.TakeTheMinutes = Info.TakeTheMinutes;
            meetingMinutes.Agenda = Info.Agenda;
            meetingMinutes.MeetingContent = Info.MeetingContent;
            meetingMinutes.MeetingFile = MeetingFile;
            meetingMinutes.UploadUserName = UserName;
            meetingMinutes.UploadDateTime = DateTime.Now;

            db.MeetingMinutes.AddOrUpdate(meetingMinutes);
            db.SaveChanges();
        }
        public void EditMeetingMinutes(MeetingMinutesInfo Info, string MeetingFile, string UserName)
        {
            var meetingMinutes = db.MeetingMinutes.Find(Info.MMSN);
            if(meetingMinutes != null)
            {
                meetingMinutes.MeetingTopic = Info.MeetingTopic;
                meetingMinutes.MeetingDate = Info.MeetingDate;
                meetingMinutes.MeetingDateStart = Info.MeetingDateStart;
                meetingMinutes.MeetingDateEnd = Info.MeetingDateEnd;
                meetingMinutes.MeetingVenue = Info.MeetingVenue;
                meetingMinutes.Chairperson = Info.Chairperson;
                meetingMinutes.Participant = Info.Participant;
                meetingMinutes.ExpectedAttendence = Info.ExpectedAttendence;
                meetingMinutes.ActualAttendence = Info.ActualAttendence;
                meetingMinutes.AbsenteeList = Info.AbsenteeList;
                meetingMinutes.TakeTheMinutes = Info.TakeTheMinutes;
                meetingMinutes.Agenda = Info.Agenda;
                meetingMinutes.MeetingContent = Info.MeetingContent;
                meetingMinutes.MeetingFile = MeetingFile;
                meetingMinutes.UploadUserName = UserName; //更新為最近一次修改者
                meetingMinutes.UploadDateTime = DateTime.Now; //更新為最近一次修改時間

                db.MeetingMinutes.AddOrUpdate(meetingMinutes);
                db.SaveChanges();
            }
        }
    }
}