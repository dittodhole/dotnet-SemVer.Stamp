using System;
using System.Collections.Generic;
using System.Linq;

namespace SemVer.Stamp.Tests
{
  public class InjectedCommitMessagesSemVersionGrabber : SemVersionGrabberBase
  {
    public InjectedCommitMessagesSemVersionGrabber()
      : base(null,
             null,
             null)
    {
    }

    public Func<IEnumerable<string>> CommitMessagesFn { get; set; }

    protected override IEnumerable<string> GetCommitMessages()
    {
      return this.CommitMessagesFn?.Invoke() ?? Enumerable.Empty<string>();
    }
  }
}
